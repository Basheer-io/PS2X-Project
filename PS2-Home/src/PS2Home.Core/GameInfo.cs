namespace PS2Home.Core;

/// <summary>
/// Represents a PS2 game or homebrew application in the library.
/// </summary>
public record GameInfo(
    string Name,
    string ConfigFolderName,
    string Tagline,
    string CoverArtPath,
    bool IsInstalled,
    string Status);
