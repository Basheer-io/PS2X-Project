namespace PS2Home.Core;

/// <summary>
/// Discovers and validates PCSX2 installations on the host system.
/// </summary>
public interface IPcsx2Locator
{
    /// <summary>
    /// Searches common platform locations for installed PCSX2 instances.
    /// Never performs full-disk scans.
    /// </summary>
    Task<IReadOnlyList<Pcsx2Installation>> DetectInstallationsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates whether a given installation exists and can be executed safely.
    /// </summary>
    Task<Pcsx2ValidationResult> ValidateInstallationAsync(
        Pcsx2Installation installation,
        CancellationToken cancellationToken = default);
}
