namespace PS2Home.Core;

/// <summary>
/// Orchestrates launching PS2 games via configured PCSX2 installations.
/// </summary>
public interface IGameLauncher
{
    /// <summary>
    /// Opens PCSX2 without starting a game.
    /// </summary>
    Task LaunchEmulatorAsync(Pcsx2Installation installation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Launches a game asynchronously using the parameters in the LaunchRequest.
    /// </summary>
    Task<IGameSession> LaunchGameAsync(LaunchRequest request, CancellationToken cancellationToken = default);
}
