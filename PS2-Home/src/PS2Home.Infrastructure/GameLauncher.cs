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

    public async Task<IGameSession> LaunchGameAsync(
        LaunchRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!File.Exists(request.GamePath))
        {
            throw new FileNotFoundException($"Game image file not found at: {request.GamePath}", request.GamePath);
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
