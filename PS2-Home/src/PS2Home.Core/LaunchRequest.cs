namespace PS2Home.Core;

/// <summary>
/// Parameters required to launch a game using PCSX2.
/// </summary>
public record LaunchRequest
{
    /// <summary>
    /// Configured PCSX2 installation to invoke.
    /// </summary>
    public required Pcsx2Installation Installation { get; init; }

    /// <summary>
    /// Target PS2 disc image or ELF file path.
    /// </summary>
    public required string GamePath { get; init; }

    /// <summary>
    /// Whether to enter fullscreen mode immediately on boot (-fullscreen).
    /// </summary>
    public bool Fullscreen { get; init; } = true;

    /// <summary>
    /// Whether to exit PCSX2 automatically when game emulation terminates (-batch).
    /// </summary>
    public bool BatchMode { get; init; } = true;

    /// <summary>
    /// Whether to suppress the main PCSX2 window (-nogui).
    /// </summary>
    public bool NoGui { get; init; } = false;
}
