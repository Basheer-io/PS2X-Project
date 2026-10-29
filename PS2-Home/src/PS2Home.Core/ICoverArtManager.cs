namespace PS2Home.Core;

/// <summary>
/// Manages user-supplied local cover art images, validation, copying to the app config directory, and fallback logic.
/// </summary>
public interface ICoverArtManager
{
    /// <summary>
    /// Gets the local directory where cover art images are stored.
    /// </summary>
    string GetCoverArtDirectory();

    /// <summary>
    /// Validates and imports a user-selected image for a game, copying it into the covers directory.
    /// Returns the destination path on success, or throws/returns null on invalid input.
    /// </summary>
    Task<string?> SetCoverArtAsync(string gameId, string sourceImagePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the custom cover art file for a game if one exists.
    /// </summary>
    Task<bool> RemoveCoverArtAsync(string gameId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves the actual usable cover art path on disk, returning null if missing, corrupt, or invalid.
    /// </summary>
    string? ResolveCoverArtPath(string gameId, string? recordedPath);

    /// <summary>
    /// Checks whether a given file path is a valid supported image within size limits.
    /// </summary>
    bool IsValidImageFile(string filePath, out string? errorMessage);
}
