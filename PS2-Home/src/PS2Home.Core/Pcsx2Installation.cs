namespace PS2Home.Core;

/// <summary>
/// Describes how to execute PCSX2 on the host system.
/// </summary>
public record Pcsx2Installation
{
    /// <summary>
    /// Type of installation.
    /// </summary>
    public Pcsx2InstallKind Kind { get; init; } = Pcsx2InstallKind.Unknown;

    /// <summary>
    /// Executable binary path or command name (e.g. "flatpak" or "/usr/bin/pcsx2").
    /// </summary>
    public string Command { get; init; } = string.Empty;

    /// <summary>
    /// Leading arguments that must precede any command-line options (e.g. ["run", "net.pcsx2.PCSX2"]).
    /// </summary>
    public IReadOnlyList<string> LeadingArguments { get; init; } = [];

    /// <summary>
    /// User-friendly display label (e.g. "PCSX2 (Flatpak)" or "PCSX2 v2.1 (Executable)").
    /// </summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>
    /// Detected version string (e.g. "2.1.0"), or null if unverified.
    /// </summary>
    public string? Version { get; init; }

    /// <summary>
    /// Whether the installation has been verified to exist and execute properly.
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// Creates a Flatpak PCSX2 installation definition.
    /// </summary>
    public static Pcsx2Installation CreateFlatpak(string? version = null) => new()
    {
        Kind = Pcsx2InstallKind.Flatpak,
        Command = "flatpak",
        LeadingArguments = ["run", "net.pcsx2.PCSX2"],
        DisplayName = "PCSX2 (Flatpak net.pcsx2.PCSX2)",
        Version = version,
        IsValid = true
    };

    /// <summary>
    /// Creates an Executable or AppImage PCSX2 installation definition.
    /// </summary>
    public static Pcsx2Installation CreateExecutable(string fullPath, Pcsx2InstallKind kind = Pcsx2InstallKind.Executable, string? version = null) => new()
    {
        Kind = kind,
        Command = fullPath,
        LeadingArguments = [],
        DisplayName = $"{Path.GetFileName(fullPath)} ({kind})",
        Version = version,
        IsValid = true
    };
}
