using System.Runtime.InteropServices;
using System.Text.Json;
using PS2Home.Core;

namespace PS2Home.Infrastructure;

/// <summary>
/// Executes the out-of-process native diagnostics helper (ps2home-diag), parses the JSON report,
/// and degrades gracefully if the utility is missing, times out, or crashes.
/// </summary>
public class NativeDiagnostics : INativeDiagnostics
{
    private readonly IProcessRunner _processRunner;
    private readonly string? _customHelperPath;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public NativeDiagnostics() : this(new DefaultProcessRunner(), null)
    {
    }

    public NativeDiagnostics(IProcessRunner processRunner, string? customHelperPath = null)
    {
        _processRunner = processRunner;
        _customHelperPath = customHelperPath;
    }

    public async Task<NativeDiagnosticsResult> QueryDiagnosticsAsync(CancellationToken cancellationToken = default)
    {
        var helperPath = LocateHelperBinary();
        if (string.IsNullOrWhiteSpace(helperPath) || !File.Exists(helperPath))
        {
            return NativeDiagnosticsResult.CreateUnavailable(
                "Native diagnostics helper (ps2home-diag) was not found. Hardware capability probe skipped.");
        }

        try
        {
            var result = await _processRunner.RunProcessAsync(
                helperPath,
                [],
                TimeSpan.FromSeconds(3),
                cancellationToken);

            if (result.TimedOut)
            {
                return NativeDiagnosticsResult.CreateUnavailable(
                    "Native hardware probe timed out after 3 seconds.");
            }

            if (string.IsNullOrWhiteSpace(result.StandardOutput))
            {
                return NativeDiagnosticsResult.CreateUnavailable(
                    $"Native probe failed (ExitCode: {result.ExitCode}): {result.StandardError}");
            }

            var parsed = JsonSerializer.Deserialize<NativeDiagnosticsResult>(result.StandardOutput, JsonOptions);
            if (parsed == null)
            {
                return NativeDiagnosticsResult.CreateUnavailable("Failed to deserialize native report.");
            }

            return parsed with { IsAvailable = true };
        }
        catch (JsonException ex)
        {
            return NativeDiagnosticsResult.CreateUnavailable($"Invalid JSON output from native helper: {ex.Message}");
        }
        catch (Exception ex)
        {
            return NativeDiagnosticsResult.CreateUnavailable($"Native helper execution failed: {ex.Message}");
        }
    }

    private string? LocateHelperBinary()
    {
        if (!string.IsNullOrWhiteSpace(_customHelperPath) && File.Exists(_customHelperPath))
            return _customHelperPath;

        var binaryName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "ps2home-diag.exe"
            : "ps2home-diag";

        var candidates = new List<string>
        {
            Path.Combine(AppContext.BaseDirectory, binaryName),
            Path.Combine(AppContext.BaseDirectory, "native", binaryName),
            Path.Combine(Environment.CurrentDirectory, "native", "PS2Home.Native", "build", "bin", binaryName),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "native", "PS2Home.Native", "build", "bin", binaryName)
        };

        foreach (var candidate in candidates)
        {
            try
            {
                var full = Path.GetFullPath(candidate);
                if (File.Exists(full))
                {
                    return full;
                }
            }
            catch
            {
                // Ignore path resolution errors
            }
        }

        // Check PATH
        var pathEnv = Environment.GetEnvironmentVariable("PATH");
        if (!string.IsNullOrWhiteSpace(pathEnv))
        {
            foreach (var dir in pathEnv.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
            {
                var full = Path.Combine(dir, binaryName);
                if (File.Exists(full))
                {
                    return full;
                }
            }
        }

        return null;
    }
}
