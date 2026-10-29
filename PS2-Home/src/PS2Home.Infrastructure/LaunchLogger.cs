using PS2Home.Core;

namespace PS2Home.Infrastructure;

/// <summary>
/// File-based launch logger storing records under AppPaths.GetConfigDirectory()/logs/launch.log.
/// Enforces a 1 MB file size cap with .old rotation and redacts user home paths.
/// </summary>
public class LaunchLogger : ILaunchLogger
{
    private readonly string _logFilePath;
    private readonly string _userHome;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private const long MaxLogSizeBytes = 1024 * 1024; // 1 MB

    public LaunchLogger() : this(AppPaths.GetConfigDirectory())
    {
    }

    public LaunchLogger(string configDirectory)
    {
        var logDir = Path.Combine(configDirectory, "logs");
        Directory.CreateDirectory(logDir);
        _logFilePath = Path.Combine(logDir, "launch.log");
        _userHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    }

    public async Task LogLaunchStartedAsync(string command, IEnumerable<string> arguments, string gamePath)
    {
        var timestamp = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff 'UTC'");
        var cleanCommand = RedactHomePath(command);
        var cleanGame = RedactHomePath(gamePath);
        var cleanArgs = string.Join(" ", arguments.Select(RedactHomePath));

        var line = $"[{timestamp}] [START] Game: \"{cleanGame}\" | Command: {cleanCommand} {cleanArgs}";
        await AppendLogLineAsync(line);
    }

    public async Task LogLaunchFinishedAsync(string gamePath, GameSessionResult result)
    {
        var timestamp = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff 'UTC'");
        var cleanGame = RedactHomePath(gamePath);
        var durationSec = result.Duration.TotalSeconds.ToString("0.00");

        var status = result.FastFailure ? "FAILED_FAST" : "EXITED";
        var line = $"[{timestamp}] [{status}] Game: \"{cleanGame}\" | ExitCode: {result.ExitCode} | Duration: {durationSec}s";

        if (!string.IsNullOrWhiteSpace(result.ErrorSummary))
        {
            var cleanError = RedactHomePath(result.ErrorSummary).Replace("\r", " ").Replace("\n", " ");
            line += $" | Error: {cleanError}";
        }

        await AppendLogLineAsync(line);
    }

    public async Task<IReadOnlyList<string>> GetRecentLogsAsync(int maxLines = 100)
    {
        await _lock.WaitAsync();
        try
        {
            if (!File.Exists(_logFilePath))
                return [];

            var lines = await File.ReadAllLinesAsync(_logFilePath);
            return lines.TakeLast(maxLines).ToList();
        }
        catch
        {
            return [];
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<string> ExportRedactedLogsAsync()
    {
        await _lock.WaitAsync();
        try
        {
            if (!File.Exists(_logFilePath))
                return "No launch logs recorded.";

            var content = await File.ReadAllTextAsync(_logFilePath);
            return RedactHomePath(content);
        }
        catch (Exception ex)
        {
            return $"Error reading logs: {ex.Message}";
        }
        finally
        {
            _lock.Release();
        }
    }

    public string RedactHomePath(string text)
    {
        if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(_userHome))
            return text;

        return text.Replace(_userHome, "~");
    }

    private async Task AppendLogLineAsync(string line)
    {
        await _lock.WaitAsync();
        try
        {
            RotateLogsIfNeeded();
            await File.AppendAllLinesAsync(_logFilePath, [line]);
        }
        catch
        {
            // Ignore disk logging failures
        }
        finally
        {
            _lock.Release();
        }
    }

    private void RotateLogsIfNeeded()
    {
        try
        {
            if (File.Exists(_logFilePath))
            {
                var fileInfo = new FileInfo(_logFilePath);
                if (fileInfo.Length > MaxLogSizeBytes)
                {
                    var backupPath = $"{_logFilePath}.old";
                    File.Move(_logFilePath, backupPath, overwrite: true);
                }
            }
        }
        catch
        {
            // Ignore rotation errors
        }
    }
}
