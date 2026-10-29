namespace PS2Home.Core;

/// <summary>
/// User configuration settings for PS2 Home.
/// </summary>
public record AppSettings
{
    /// <summary>
    /// List of root directories to scan for PS2 game images.
    /// </summary>
    public List<string> GameFolders { get; init; } = [];

    /// <summary>
    /// Directory containing cover art image files.
    /// </summary>
    public string CoverArtFolder { get; init; } = string.Empty;

    /// <summary>
    /// Whether to automatically scan configured folders when the application starts.
    /// </summary>
    public bool AutoScanOnStartup { get; init; } = true;

    /// <summary>
    /// Whether to show hidden or missing items.
    /// </summary>
    public bool ShowHiddenApps { get; init; } = false;

    /// <summary>
    /// UI theme name (Dark, Light, System).
    /// </summary>
    public string Theme { get; init; } = "Dark";
}
