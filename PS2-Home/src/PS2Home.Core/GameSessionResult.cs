namespace PS2Home.Core;

/// <summary>
/// Result captured upon termination of a game session.
/// </summary>
public record GameSessionResult(
    int ExitCode,
    TimeSpan Duration,
    string? ErrorSummary,
    bool FastFailure);
