using System.Diagnostics;
using PS2Home.Core;

namespace PS2Home.Infrastructure;

/// <summary>
/// Monitors a running PCSX2 game process and detects rapid exits or crashes.
/// </summary>
public class GameSession : IGameSession
{
    private readonly Process _process;
    private readonly TaskCompletionSource<GameSessionResult> _tcs = new();
    private readonly System.Text.StringBuilder _stderrBuffer = new();
    private readonly ILaunchLogger? _logger;

    public string GamePath { get; }
    public GameLaunchState State { get; private set; }
    public DateTimeOffset StartedAt { get; }

    public GameSession(
        Process process,
        string gamePath,
        ILaunchLogger? logger = null)
    {
        _process = process;
        GamePath = gamePath;
        _logger = logger;
        StartedAt = DateTimeOffset.UtcNow;
        State = GameLaunchState.Running;

        _process.EnableRaisingEvents = true;
        _process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data != null)
            {
                lock (_stderrBuffer)
                {
                    if (_stderrBuffer.Length < 4096)
                    {
                        _stderrBuffer.AppendLine(e.Data);
                    }
                }
            }
        };

        _process.Exited += OnProcessExited;

        try
        {
            _process.BeginErrorReadLine();
        }
        catch
        {
            // Ignore error stream setup failures if not redirected
        }
    }

    public Task<GameSessionResult> WaitForExitAsync() => _tcs.Task;

    public void Terminate()
    {
        try
        {
            if (!_process.HasExited)
            {
                _process.Kill(entireProcessTree: true);
            }
        }
        catch
        {
            // Ignore termination errors if process has already stopped
        }
    }

    private void OnProcessExited(object? sender, EventArgs e)
    {
        var duration = DateTimeOffset.UtcNow - StartedAt;
        var exitCode = _process.ExitCode;

        string stderr;
        lock (_stderrBuffer)
        {
            stderr = _stderrBuffer.ToString().Trim();
        }

        // Fast failure: exits non-zero in less than 3 seconds from launch
        bool isFastFailure = exitCode != 0 && duration < TimeSpan.FromSeconds(3);

        State = isFastFailure ? GameLaunchState.Failed : GameLaunchState.Exited;

        var result = new GameSessionResult(
            exitCode,
            duration,
            string.IsNullOrWhiteSpace(stderr) ? null : stderr,
            isFastFailure);

        _logger?.LogLaunchFinishedAsync(GamePath, result);

        _tcs.TrySetResult(result);
    }
}
