namespace PS2Home.Core;

/// <summary>
/// Represents an active or completed game process session.
/// </summary>
public interface IGameSession
{
    /// <summary>
    /// File path of the game being emulated.
    /// </summary>
    string GamePath { get; }

    /// <summary>
    /// Current state of the game process.
    /// </summary>
    GameLaunchState State { get; }

    /// <summary>
    /// Timestamp when process execution began.
    /// </summary>
    DateTimeOffset StartedAt { get; }

    /// <summary>
    /// Awaits process termination and yields the final exit code and diagnostics.
    /// </summary>
    Task<GameSessionResult> WaitForExitAsync();

    /// <summary>
    /// Requests immediate termination of the emulator process.
    /// </summary>
    void Terminate();
}
