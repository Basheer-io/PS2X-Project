namespace PS2Home.Core;

/// <summary>
/// Probes low-level hardware capabilities via the standalone native helper utility.
/// </summary>
public interface INativeDiagnostics
{
    /// <summary>
    /// Queries CPU instructions and Vulkan capabilities.
    /// Degrades gracefully if the native helper is not compiled or fails.
    /// </summary>
    Task<NativeDiagnosticsResult> QueryDiagnosticsAsync(CancellationToken cancellationToken = default);
}
