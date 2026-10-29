namespace PS2Home.Core;

/// <summary>
/// Composes verified command-line argument lists for PCSX2 2.x Qt.
/// Follows documented flags (-batch, -fullscreen, -nogui) and uses the '--'
/// option separator to safely delimit game file paths.
/// Never concatenates shell command strings.
/// </summary>
public static class Pcsx2ArgumentBuilder
{
    /// <summary>
    /// Builds the complete ArgumentList for launching PCSX2.
    /// </summary>
    public static IReadOnlyList<string> BuildArguments(LaunchRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.GamePath))
            throw new ArgumentException("Game path must not be empty.", nameof(request));

        if (request.Installation == null || string.IsNullOrWhiteSpace(request.Installation.Command))
            throw new ArgumentException("PCSX2 installation command must not be empty.", nameof(request));

        var args = new List<string>();

        // 1. Leading arguments from installation (e.g. ["run", "net.pcsx2.PCSX2"] for Flatpak)
        if (request.Installation.LeadingArguments != null && request.Installation.LeadingArguments.Count > 0)
        {
            args.AddRange(request.Installation.LeadingArguments);
        }

        // 2. Documented PCSX2 2.x CLI options
        if (request.BatchMode)
        {
            args.Add("-batch");
        }

        if (request.Fullscreen)
        {
            args.Add("-fullscreen");
        }

        if (request.NoGui)
        {
            args.Add("-nogui");
        }

        // 3. Option terminator '--' ensures any leading dashes in filename are treated as file paths
        args.Add("--");

        // 4. Game path as a single distinct argument
        args.Add(request.GamePath);

        return args;
    }
}
