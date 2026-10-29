using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using PS2Home.Core;

namespace PS2Home.Infrastructure;

/// <summary>
/// Discovers and validates PCSX2 instances across Linux and Windows platforms.
/// Never performs full disk scans; queries standard package managers and known locations only.
/// </summary>
public partial class Pcsx2Locator : IPcsx2Locator
{
    private readonly IProcessRunner _processRunner;

    [GeneratedRegex(@"\b(v?\d+\.\d+(\.\d+)?(-[a-zA-Z0-9]+)?)\b", RegexOptions.IgnoreCase)]
    private static partial Regex VersionRegex();

    public Pcsx2Locator() : this(new DefaultProcessRunner())
    {
    }

    public Pcsx2Locator(IProcessRunner processRunner)
    {
        _processRunner = processRunner;
    }

    public async Task<IReadOnlyList<Pcsx2Installation>> DetectInstallationsAsync(CancellationToken cancellationToken = default)
    {
        var installations = new List<Pcsx2Installation>();
        var seenPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            await DetectLinuxInstallationsAsync(installations, seenPaths, cancellationToken);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            await DetectWindowsInstallationsAsync(installations, seenPaths, cancellationToken);
        }

        return installations;
    }

    public async Task<Pcsx2ValidationResult> ValidateInstallationAsync(
        Pcsx2Installation installation,
        CancellationToken cancellationToken = default)
    {
        if (installation == null || string.IsNullOrWhiteSpace(installation.Command))
        {
            return new Pcsx2ValidationResult(false, null, null, "No command or executable path specified.");
        }

        if (installation.Kind == Pcsx2InstallKind.Flatpak)
        {
            return await ValidateFlatpakAsync(cancellationToken);
        }

        // Direct executable or AppImage validation
        if (!File.Exists(installation.Command))
        {
            return new Pcsx2ValidationResult(false, null, null, $"Binary not found at: {installation.Command}");
        }

        // On Unix, check execute permission
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            try
            {
                var mode = File.GetUnixFileMode(installation.Command);
                var isExecutable = (mode & (UnixFileMode.UserExecute | UnixFileMode.GroupExecute | UnixFileMode.OtherExecute)) != 0;
                if (!isExecutable)
                {
                    return new Pcsx2ValidationResult(false, null, null, "File is not marked executable. Make it executable with 'chmod +x'.");
                }
            }
            catch
            {
                // Proceed if file mode query fails
            }
        }

        // Run query with a strict 2-second timeout
        var args = installation.LeadingArguments.Concat(["--version"]);
        var result = await _processRunner.RunProcessAsync(
            installation.Command,
            args,
            TimeSpan.FromSeconds(2),
            cancellationToken);

        if (result.TimedOut)
        {
            return new Pcsx2ValidationResult(false, null, null, "Query timed out after 2 seconds.");
        }

        var combinedOutput = $"{result.StandardOutput} {result.StandardError}".Trim();
        var version = ExtractVersion(combinedOutput);

        if (result.ExitCode == 0 || combinedOutput.Contains("PCSX2", StringComparison.OrdinalIgnoreCase))
        {
            return new Pcsx2ValidationResult(true, version, combinedOutput, null);
        }

        return new Pcsx2ValidationResult(
            false,
            null,
            combinedOutput,
            $"Process exited with code {result.ExitCode}: {result.StandardError}");
    }

    private async Task DetectLinuxInstallationsAsync(
        List<Pcsx2Installation> installations,
        HashSet<string> seenPaths,
        CancellationToken cancellationToken)
    {
        // 1. Check Flatpak
        var flatpakValidation = await ValidateFlatpakAsync(cancellationToken);
        if (flatpakValidation.IsValid)
        {
            installations.Add(Pcsx2Installation.CreateFlatpak(flatpakValidation.Version));
        }

        // 2. Check PATH and standard directories
        var searchBins = new[] { "pcsx2-qt", "pcsx2" };
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        var directPaths = new List<string>
        {
            "/usr/bin/pcsx2-qt",
            "/usr/bin/pcsx2",
            "/usr/local/bin/pcsx2-qt",
            "/usr/local/bin/pcsx2",
            Path.Combine(home, ".local", "bin", "pcsx2-qt"),
            Path.Combine(home, ".local", "bin", "pcsx2")
        };

        // AppImage common directories
        var appImageDirs = new[]
        {
            Path.Combine(home, "Applications"),
            Path.Combine(home, "Desktop"),
            Path.Combine(home, ".local", "bin")
        };

        foreach (var dir in appImageDirs)
        {
            if (Directory.Exists(dir))
            {
                try
                {
                    foreach (var file in Directory.EnumerateFiles(dir, "*pcsx2*.AppImage", SearchOption.TopDirectoryOnly))
                    {
                        directPaths.Add(file);
                    }
                }
                catch
                {
                    // Ignore inaccessible directories
                }
            }
        }

        // Add matches from PATH
        var pathEnv = Environment.GetEnvironmentVariable("PATH");
        if (!string.IsNullOrWhiteSpace(pathEnv))
        {
            foreach (var dir in pathEnv.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
            {
                foreach (var bin in searchBins)
                {
                    directPaths.Add(Path.Combine(dir, bin));
                }
            }
        }

        foreach (var candidate in directPaths)
        {
            if (seenPaths.Add(candidate) && File.Exists(candidate))
            {
                var kind = candidate.EndsWith(".AppImage", StringComparison.OrdinalIgnoreCase)
                    ? Pcsx2InstallKind.AppImage
                    : Pcsx2InstallKind.Executable;

                var validation = await ValidateInstallationAsync(
                    Pcsx2Installation.CreateExecutable(candidate, kind),
                    cancellationToken);

                if (validation.IsValid)
                {
                    installations.Add(Pcsx2Installation.CreateExecutable(candidate, kind, validation.Version));
                }
            }
        }
    }

    private async Task DetectWindowsInstallationsAsync(
        List<Pcsx2Installation> installations,
        HashSet<string> seenPaths,
        CancellationToken cancellationToken)
    {
        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        var candidates = new List<string>
        {
            Path.Combine(programFiles, "PCSX2", "pcsx2-qt.exe"),
            Path.Combine(programFiles, "PCSX2", "pcsx2.exe"),
            Path.Combine(programFilesX86, "PCSX2", "pcsx2-qt.exe"),
            Path.Combine(programFilesX86, "PCSX2", "pcsx2.exe"),
            Path.Combine(localAppData, "Programs", "PCSX2", "pcsx2-qt.exe")
        };

        var pathEnv = Environment.GetEnvironmentVariable("PATH");
        if (!string.IsNullOrWhiteSpace(pathEnv))
        {
            foreach (var dir in pathEnv.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
            {
                candidates.Add(Path.Combine(dir, "pcsx2-qt.exe"));
                candidates.Add(Path.Combine(dir, "pcsx2.exe"));
            }
        }

        foreach (var candidate in candidates)
        {
            if (seenPaths.Add(candidate) && File.Exists(candidate))
            {
                var validation = await ValidateInstallationAsync(
                    Pcsx2Installation.CreateExecutable(candidate, Pcsx2InstallKind.Executable),
                    cancellationToken);

                if (validation.IsValid)
                {
                    installations.Add(Pcsx2Installation.CreateExecutable(candidate, Pcsx2InstallKind.Executable, validation.Version));
                }
            }
        }
    }

    private async Task<Pcsx2ValidationResult> ValidateFlatpakAsync(CancellationToken cancellationToken)
    {
        var result = await _processRunner.RunProcessAsync(
            "flatpak",
            ["info", "net.pcsx2.PCSX2"],
            TimeSpan.FromSeconds(2),
            cancellationToken);

        if (result.ExitCode == 0 && !result.TimedOut)
        {
            var version = ExtractVersion(result.StandardOutput);
            return new Pcsx2ValidationResult(true, version, result.StandardOutput, null);
        }

        return new Pcsx2ValidationResult(false, null, null, "Flatpak package net.pcsx2.PCSX2 is not installed.");
    }

    private static string? ExtractVersion(string output)
    {
        if (string.IsNullOrWhiteSpace(output))
            return null;

        var match = VersionRegex().Match(output);
        return match.Success ? match.Groups[1].Value : null;
    }
}
