namespace PS2Home.Core;

/// <summary>
/// Scans specified directories for PS2 game disc images.
/// </summary>
public interface IGameScanner
{
    /// <summary>
    /// Supported file extensions for game images (case-insensitive).
    /// </summary>
    IReadOnlyList<string> SupportedExtensions { get; }

    /// <summary>
    /// Scans directories recursively for compatible PS2 game files.
    /// </summary>
    Task<IReadOnlyList<GameEntry>> ScanFoldersAsync(
        IEnumerable<string> folderPaths,
        IProgress<ScanProgress>? progress = null,
        CancellationToken cancellationToken = default);
}
