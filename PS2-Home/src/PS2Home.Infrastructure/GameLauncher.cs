using System.Diagnostics;
using PS2Home.Core;

namespace PS2Home.Infrastructure;

/// <summary>
/// Launches PCSX2 processes with ArgumentList strictly.
/// Handles Flatpak permission considerations and logs lifecycle events.
/// </summary>
public class GameLauncher : IGameLauncher
{
    private readonly ILaunchLogger _launchLogger;

    public GameLauncher(ILaunchLogger launchLogger)
    {
        _launchLogger = launchLogger;
    }

    public Task LaunchEmulatorAsync(
        Pcsx2Installation installation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(installation);
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(installation.Command))
        {
            throw new InvalidOperationException("PCSX2 is not configured with a command or executable path.");
        }

        if (installation.Kind != Pcsx2InstallKind.Flatpak && !File.Exists(installation.Command))
        {
            throw new FileNotFoundException(
                $"PCSX2 executable not found at: {installation.Command}",
                installation.Command);
        }

        var psi = new ProcessStartInfo
        {
            FileName = installation.Command,
            UseShellExecute = false,
            CreateNoWindow = false
        };

        foreach (var argument in installation.LeadingArguments)
        {
            psi.ArgumentList.Add(argument);
        }

        var process = new Process { StartInfo = psi };
        if (!process.Start())
        {
            process.Dispose();
            throw new InvalidOperationException($"Failed to start PCSX2 using: {installation.Command}");
        }

        process.Dispose();
        return Task.CompletedTask;
    }

    public async Task<IGameSession> LaunchGameAsync(
        LaunchRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Installation);
        cancellationToken.ThrowIfCancellationRequested();

        if (!File.Exists(request.GamePath))
        {
            throw new FileNotFoundException($"Game image file not found at: {request.GamePath}", request.GamePath);
        }

        if (string.IsNullOrWhiteSpace(request.Installation.Command))
        {
            throw new InvalidOperationException("PCSX2 is not configured with a command or executable path.");
        }

        if (request.Installation.Kind != Pcsx2InstallKind.Flatpak &&
            !File.Exists(request.Installation.Command))
        {
            throw new FileNotFoundException(
                $"PCSX2 executable not found at: {request.Installation.Command}",
                request.Installation.Command);
        }

        var args = Pcsx2ArgumentBuilder.BuildArguments(request);

        // Check for Linux Flatpak sandbox permission considerations
        if (request.Installation.Kind == Pcsx2InstallKind.Flatpak)
        {
            CheckFlatpakSandboxPermissions(request.GamePath);
        }

        var psi = new ProcessStartInfo
        {
            FileName = request.Installation.Command,
            UseShellExecute = false,
            CreateNoWindow = false,
            RedirectStandardError = true,
            WorkingDirectory = Path.GetDirectoryName(request.GamePath) ?? Environment.CurrentDirectory
        };

        foreach (var arg in args)
        {
            psi.ArgumentList.Add(arg);
        }

        await _launchLogger.LogLaunchStartedAsync(request.Installation.Command, args, request.GamePath);

        var process = new Process { StartInfo = psi };

        if (!process.Start())
        {
            throw new InvalidOperationException($"Failed to start PCSX2 process using command: {request.Installation.Command}");
        }

        return new GameSession(process, request.GamePath, _launchLogger);
    }

    private void CheckFlatpakSandboxPermissions(string gamePath)
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (!gamePath.StartsWith(home, StringComparison.Ordinal))
        {
            // Path is outside /home; Flatpak may be blocked from reading it unless filesystem overrides are configured
            var dir = Path.GetDirectoryName(gamePath) ?? gamePath;
            var hint = $"Note: For Flatpak to read external directories like '{dir}', grant access via: flatpak override --user --filesystem=\"{dir}\" net.pcsx2.PCSX2";
            _ = _launchLogger.LogLaunchStartedAsync("flatpak", ["info", hint], gamePath);
        }
    }
}
