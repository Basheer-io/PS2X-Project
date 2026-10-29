namespace PS2Home.Core;

/// <summary>
/// Method of PCSX2 installation on the host operating system.
/// </summary>
public enum Pcsx2InstallKind
{
    /// <summary>
    /// Native binary on PATH or standard executable (.exe / ELF).
    /// </summary>
    Executable,

    /// <summary>
    /// Standalone Linux AppImage bundle.
    /// </summary>
    AppImage,

    /// <summary>
    /// Sandboxed Linux Flatpak package (net.pcsx2.PCSX2).
    /// </summary>
    Flatpak,

    /// <summary>
    /// User-specified custom binary or script.
    /// </summary>
    Custom,

    /// <summary>
    /// Unknown or unverified installation.
    /// </summary>
    Unknown
}
