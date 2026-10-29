namespace PS2Home.Core;

/// <summary>
/// Result of verifying a PCSX2 installation.
/// </summary>
public record Pcsx2ValidationResult(
    bool IsValid,
    string? Version,
    string? Output,
    string? ErrorMessage);
