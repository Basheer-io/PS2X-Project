namespace PS2Home.Core;

/// <summary>Static application metadata shared across projects.</summary>
public static class AppInfo
{
    public const string Name = "PS2 Home";

    public const string Version = "1.0.0";

    /// <summary>Folder name used under the platform config directory.</summary>
    public const string ConfigFolderName = "PS2Home";

    public const string Tagline = "A PS2 game launcher and setup assistant";

    /// <summary>
    /// Explains the runtime boundary: PS2 software cannot execute directly on a
    /// general-purpose PC because it targets PS2-specific hardware.
    /// </summary>
    public const string RuntimeNotice =
        "PS2 games cannot run natively on a normal PC. PS2 Home launches them through your installed PCSX2 emulator.";
}
