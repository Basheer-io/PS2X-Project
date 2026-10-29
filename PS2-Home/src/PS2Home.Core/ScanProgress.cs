namespace PS2Home.Core;

/// <summary>
/// Status report produced during library scanning.
/// </summary>
public record ScanProgress(
    int FoldersScanned,
    int GamesFound,
    string CurrentPath);
