namespace PS2Home.Core;

/// <summary>
/// Logs game launch events, commands, exit codes, and durations.
/// Redacts personal home paths for safe sharing.
/// </summary>
public interface ILaunchLogger
{
    /// <summary>
    /// Logs a launch initiation event.
    /// </summary>
    Task LogLaunchStartedAsync(string command, IEnumerable<string> arguments, string gamePath);

    /// <summary>
    /// Logs a session termination event.
    /// </summary>
    Task LogLaunchFinishedAsync(string gamePath, GameSessionResult result);

    /// <summary>
    /// Reads recent launch log lines.
    /// </summary>
    Task<IReadOnlyList<string>> GetRecentLogsAsync(int maxLines = 100);

    /// <summary>
    /// Exports all launch logs with home paths redacted.
    /// </summary>
    Task<string> ExportRedactedLogsAsync();
}
