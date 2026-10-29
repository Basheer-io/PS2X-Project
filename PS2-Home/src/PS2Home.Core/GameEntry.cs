namespace PS2Home.Core;

/// <summary>
/// Represents a scanned PS2 game disc image or homebrew entry in the library.
/// </summary>
public record GameEntry
{
    /// <summary>
    /// Unique identifier based on the normalized file path.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Display title for the game, derived from filename or user-customized.
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Full normalized filesystem path to the game image.
    /// </summary>
    public string FullPath { get; init; } = string.Empty;

    /// <summary>
    /// File size in bytes.
    /// </summary>
    public long FileSizeBytes { get; init; }

    /// <summary>
    /// Timestamp when this game was first discovered and added to the library.
    /// </summary>
    public DateTimeOffset AddedAt { get; init; }

    /// <summary>
    /// Timestamp when the game image was last verified on disk.
    /// </summary>
    public DateTimeOffset LastScannedAt { get; init; }

    /// <summary>
    /// True if the game file could not be found at FullPath during the last scan.
    /// </summary>
    public bool IsMissing { get; init; }

    public static string CreateId(string fullPath) =>
        Path.GetFullPath(fullPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
}
