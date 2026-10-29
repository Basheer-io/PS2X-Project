using PS2Home.Core;

namespace PS2Home.Infrastructure;

/// <summary>
/// Production implementation of IGameLibrary orchestrating folder scanning and JSON persistence.
/// </summary>
public class RealGameLibrary : IGameLibrary
{
    private readonly ILibraryStore _libraryStore;
    private readonly IGameScanner _gameScanner;

    public RealGameLibrary(ILibraryStore libraryStore, IGameScanner gameScanner)
    {
        _libraryStore = libraryStore;
        _gameScanner = gameScanner;
    }

    public async Task<IReadOnlyList<GameEntry>> GetGamesAsync(CancellationToken cancellationToken = default)
    {
        return await _libraryStore.LoadLibraryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<GameEntry>> RescanAsync(
        IEnumerable<string> folderPaths,
        IProgress<ScanProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var existing = await _libraryStore.LoadLibraryAsync(cancellationToken);
        var existingMap = existing.ToDictionary(g => g.Id, StringComparer.OrdinalIgnoreCase);

        var scanned = await _gameScanner.ScanFoldersAsync(folderPaths, progress, cancellationToken);
        var scannedMap = scanned.ToDictionary(g => g.Id, StringComparer.OrdinalIgnoreCase);

        var merged = new List<GameEntry>();

        // Incorporate freshly scanned games (preserving AddedAt and customized titles)
        foreach (var fresh in scanned)
        {
            if (existingMap.TryGetValue(fresh.Id, out var prev))
            {
                merged.Add(fresh with
                {
                    AddedAt = prev.AddedAt,
                    Title = string.IsNullOrWhiteSpace(prev.Title) ? fresh.Title : prev.Title
                });
            }
            else
            {
                merged.Add(fresh);
            }
        }

        // Check games from previous scans that were not re-scanned (e.g. from folders not currently scanned or removed)
        foreach (var (id, prev) in existingMap)
        {
            if (!scannedMap.ContainsKey(id))
            {
                bool exists = File.Exists(prev.FullPath);
                merged.Add(prev with
                {
                    IsMissing = !exists,
                    LastScannedAt = DateTimeOffset.UtcNow
                });
            }
        }

        await _libraryStore.SaveLibraryAsync(merged, cancellationToken);
        return merged;
    }

    public async Task<IReadOnlyList<GameInfo>> GetAllGamesAsync()
    {
        var games = await GetGamesAsync();
        return games.Select(g => new GameInfo(
            g.Title,
            Path.GetFileName(Path.GetDirectoryName(g.FullPath)) ?? "",
            g.IsMissing ? "Missing file" : $"{g.FileSizeBytes / (1024 * 1024)} MB",
            "",
            !g.IsMissing,
            g.IsMissing ? "Missing" : "Ready")).ToList();
    }
}
