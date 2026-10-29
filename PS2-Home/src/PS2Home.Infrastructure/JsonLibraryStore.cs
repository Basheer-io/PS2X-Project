using System.Text.Json;
using PS2Home.Core;

namespace PS2Home.Infrastructure;

/// <summary>
/// Persists and loads the scanned game library to/from JSON with atomic file writes
/// and automated backup upon encountering corrupt JSON.
/// </summary>
public class JsonLibraryStore : ILibraryStore
{
    private readonly string _libraryFilePath;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public JsonLibraryStore() : this(AppPaths.GetConfigDirectory())
    {
    }

    public JsonLibraryStore(string configDirectory)
    {
        Directory.CreateDirectory(configDirectory);
        _libraryFilePath = Path.Combine(configDirectory, "library.json");
    }

    public async Task<IReadOnlyList<GameEntry>> LoadLibraryAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_libraryFilePath))
        {
            return [];
        }

        try
        {
            var json = await File.ReadAllTextAsync(_libraryFilePath, cancellationToken);
            if (string.IsNullOrWhiteSpace(json))
                return [];

            var games = JsonSerializer.Deserialize<List<GameEntry>>(json, JsonOptions);
            return games ?? [];
        }
        catch (JsonException)
        {
            BackupCorruptedFile();
            return [];
        }
    }

    public async Task SaveLibraryAsync(IEnumerable<GameEntry> games, CancellationToken cancellationToken = default)
    {
        var tempFilePath = $"{_libraryFilePath}.tmp.{Guid.NewGuid():N}";
        try
        {
            var gameList = games.ToList();
            var json = JsonSerializer.Serialize(gameList, JsonOptions);
            await File.WriteAllTextAsync(tempFilePath, json, cancellationToken);

            File.Move(tempFilePath, _libraryFilePath, overwrite: true);
        }
        finally
        {
            if (File.Exists(tempFilePath))
            {
                try { File.Delete(tempFilePath); } catch { }
            }
        }
    }

    private void BackupCorruptedFile()
    {
        try
        {
            var backupPath = $"{_libraryFilePath}.corrupt.{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}.bak";
            File.Copy(_libraryFilePath, backupPath, overwrite: true);
        }
        catch
        {
            // Ignore backup failures
        }
    }
}
