using PS2Home.Core;

namespace PS2Home.Tests;

/// <summary>
/// Test double for IProcessRunner allowing simulated process outcomes, streams, and timeouts.
/// </summary>
public class FakeProcessRunner : IProcessRunner
{
    public Func<string, IEnumerable<string>, ProcessExecutionResult>? Handler { get; set; }

    public List<(string FileName, List<string> Arguments)> ExecutedCalls { get; } = [];

    public Task<ProcessExecutionResult> RunProcessAsync(
        string fileName,
        IEnumerable<string> arguments,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        var argList = arguments.ToList();
        ExecutedCalls.Add((fileName, argList));

        if (Handler != null)
        {
            return Task.FromResult(Handler(fileName, argList));
        }

        return Task.FromResult(new ProcessExecutionResult(0, string.Empty, string.Empty, false));
    }
}
