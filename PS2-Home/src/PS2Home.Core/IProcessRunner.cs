namespace PS2Home.Core;

/// <summary>
/// Output produced by a process execution.
/// </summary>
public record ProcessExecutionResult(
    int ExitCode,
    string StandardOutput,
    string StandardError,
    bool TimedOut);

/// <summary>
/// Executes external processes safely using ArgumentList with timeouts and zero shell expansion.
/// </summary>
public interface IProcessRunner
{
    /// <summary>
    /// Executes a process with strictly typed arguments and a timeout.
    /// Never invokes a shell.
    /// </summary>
    Task<ProcessExecutionResult> RunProcessAsync(
        string fileName,
        IEnumerable<string> arguments,
        TimeSpan timeout,
        CancellationToken cancellationToken = default);
}
