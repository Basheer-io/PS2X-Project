namespace PS2Home.Core;

/// <summary>
/// Provides access to the PS2 game library and scanning orchestration.
/// </summary>
public interface IGameLibrary
{
    /// <summary>
    /// Returns the currently cached/persisted games.
    /// </summary>
    Task<IReadOnlyList<GameEntry>> GetGamesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Scans the given folders, merges with existing entries (flagging missing ones),
    /// persists the updated library, and returns all entries.
    /// </summary>
    Task<IReadOnlyList<GameEntry>> RescanAsync(
        IEnumerable<string> folderPaths,
        IProgress<ScanProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Legacy compatibility method for GameInfo projection.
    /// </summary>
    Task<IReadOnlyList<GameInfo>> GetAllGamesAsync();
}
