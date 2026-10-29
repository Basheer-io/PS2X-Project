using PS2Home.Core;

namespace PS2Home.Infrastructure;

/// <summary>
/// Manages user-selected cover art files in the application's config directory.
/// </summary>
public class CoverArtManager : ICoverArtManager
{
    public const long MaxCoverSizeBytes = 10 * 1024 * 1024; // 10 MB
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png", ".jpg", ".jpeg", ".webp", ".bmp"
    };

    private readonly string _coversDirectory;

    public CoverArtManager() : this(AppPaths.GetCoversDirectory())
    {
    }

    public CoverArtManager(string coversDirectory)
    {
        _coversDirectory = string.IsNullOrWhiteSpace(coversDirectory)
            ? AppPaths.GetCoversDirectory()
            : coversDirectory;
    }

    public string GetCoverArtDirectory() => _coversDirectory;

    public bool IsValidImageFile(string filePath, out string? errorMessage)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            errorMessage = "Image path is empty.";
            return false;
        }

        if (!File.Exists(filePath))
        {
            errorMessage = $"Image file does not exist at '{filePath}'.";
            return false;
        }

        var ext = Path.GetExtension(filePath);
        if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))
        {
            errorMessage = $"Unsupported image format '{ext}'. Allowed: {string.Join(", ", AllowedExtensions)}";
            return false;
        }

        var info = new FileInfo(filePath);
        if (info.Length == 0)
        {
            errorMessage = "Image file is empty (0 bytes).";
            return false;
        }

        if (info.Length > MaxCoverSizeBytes)
        {
            errorMessage = $"Image file exceeds maximum allowed size ({info.Length / (1024 * 1024)} MB > {MaxCoverSizeBytes / (1024 * 1024)} MB).";
            return false;
        }

        errorMessage = null;
        return true;
    }

    public async Task<string?> SetCoverArtAsync(string gameId, string sourceImagePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(gameId))
            throw new ArgumentException("Game ID must not be empty.", nameof(gameId));

        if (!IsValidImageFile(sourceImagePath, out var error))
            throw new ArgumentException(error ?? "Invalid image file.", nameof(sourceImagePath));

        Directory.CreateDirectory(_coversDirectory);

        var safeId = SanitizeFileName(gameId);
        var ext = Path.GetExtension(sourceImagePath).ToLowerInvariant();
        var destFileName = $"{safeId}{ext}";
        var destFilePath = Path.Combine(_coversDirectory, destFileName);

        // Remove any existing covers with other extensions for this game
        RemoveExistingCoversForId(safeId);

        // Copy atomically using temp file
        var tempFilePath = $"{destFilePath}.{Guid.NewGuid():N}.tmp";
        await using (var sourceStream = new FileStream(sourceImagePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true))
        await using (var targetStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
        {
            await sourceStream.CopyToAsync(targetStream, cancellationToken);
        }

        File.Move(tempFilePath, destFilePath, overwrite: true);
        return destFilePath;
    }

    public Task<bool> RemoveCoverArtAsync(string gameId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(gameId))
            return Task.FromResult(false);

        var safeId = SanitizeFileName(gameId);
        bool removed = RemoveExistingCoversForId(safeId);
        return Task.FromResult(removed);
    }

    public string? ResolveCoverArtPath(string gameId, string? recordedPath)
    {
        if (!string.IsNullOrWhiteSpace(recordedPath) && File.Exists(recordedPath))
        {
            try
            {
                var length = new FileInfo(recordedPath).Length;
                if (length > 0)
                    return recordedPath;
            }
            catch
            {
                // Unreadable or corrupt
            }
        }

        // Check if there is an image in the covers directory matching this sanitized game ID
        if (!string.IsNullOrWhiteSpace(gameId) && Directory.Exists(_coversDirectory))
        {
            var safeId = SanitizeFileName(gameId);
            foreach (var ext in AllowedExtensions)
            {
                var candidate = Path.Combine(_coversDirectory, $"{safeId}{ext}");
                if (File.Exists(candidate))
                {
                    try
                    {
                        if (new FileInfo(candidate).Length > 0)
                            return candidate;
                    }
                    catch
                    {
                        // Ignore corrupt files
                    }
                }
            }
        }

        return null;
    }

    private bool RemoveExistingCoversForId(string safeId)
    {
        if (!Directory.Exists(_coversDirectory))
            return false;

        bool removedAny = false;
        foreach (var ext in AllowedExtensions)
        {
            var path = Path.Combine(_coversDirectory, $"{safeId}{ext}");
            if (File.Exists(path))
            {
                try
                {
                    File.Delete(path);
                    removedAny = true;
                }
                catch
                {
                    // Ignore deletion failures
                }
            }
        }
        return removedAny;
    }

    private static string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var chars = name.Select(c => invalid.Contains(c) ? '_' : c).ToArray();
        var sanitized = new string(chars).Trim('_');
        return string.IsNullOrWhiteSpace(sanitized) ? "game_cover" : sanitized;
    }
}
