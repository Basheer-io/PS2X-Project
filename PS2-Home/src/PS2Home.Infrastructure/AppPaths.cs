using PS2Home.Core;

namespace PS2Home.Infrastructure;

/// <summary>
/// Resolves where PS2 Home stores its files. Uses the platform config directory
/// (~/.config or $XDG_CONFIG_HOME on Linux, %APPDATA% on Windows), never the app folder.
/// </summary>
public static class AppPaths
{
    public static string GetConfigDirectory() =>
        GetConfigDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));

    /// <summary>Overload taking a base directory so it can be tested without touching real user folders.</summary>
    public static string GetConfigDirectory(string baseDirectory)
    {
        if (string.IsNullOrWhiteSpace(baseDirectory))
            throw new ArgumentException("Base directory must not be empty.", nameof(baseDirectory));

        return Path.Combine(baseDirectory, AppInfo.ConfigFolderName);
    }
}
