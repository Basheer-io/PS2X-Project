using System.ComponentModel;
using System.Diagnostics;
using PS2Home.Core;

namespace PS2Home.Infrastructure;

/// <summary>
/// Safe process runner utilizing ProcessStartInfo.ArgumentList strictly.
/// Prevents shell injection, captures standard streams, and enforces timeouts.
/// </summary>
public class DefaultProcessRunner : IProcessRunner
{
    public async Task<ProcessExecutionResult> RunProcessAsync(
        string fileName,
        IEnumerable<string> arguments,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("Process filename must not be empty.", nameof(fileName));

        var psi = new ProcessStartInfo
        {
            FileName = fileName,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        foreach (var arg in arguments)
        {
            psi.ArgumentList.Add(arg);
        }

        using var process = new Process { StartInfo = psi };
        using var timeoutCts = new CancellationTokenSource(timeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        try
        {
            if (!process.Start())
            {
                return new ProcessExecutionResult(-1, string.Empty, "Failed to start process.", false);
            }

            var stdoutTask = process.StandardOutput.ReadToEndAsync(linkedCts.Token);
            var stderrTask = process.StandardError.ReadToEndAsync(linkedCts.Token);

            await process.WaitForExitAsync(linkedCts.Token);

            var stdout = await stdoutTask;
            var stderr = await stderrTask;

            return new ProcessExecutionResult(process.ExitCode, stdout, stderr, false);
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
        {
            TryKillProcess(process);
            return new ProcessExecutionResult(-1, string.Empty, "Execution timed out.", true);
        }
        catch (OperationCanceledException)
        {
            TryKillProcess(process);
            throw;
        }
        catch (Win32Exception ex)
        {
            return new ProcessExecutionResult(-1, string.Empty, $"Failed to launch binary: {ex.Message}", false);
        }
        catch (Exception ex)
        {
            return new ProcessExecutionResult(-1, string.Empty, $"Execution failed: {ex.Message}", false);
        }
    }

    private static void TryKillProcess(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch
        {
            // Ignore failure to terminate already-dead process
        }
    }
}
