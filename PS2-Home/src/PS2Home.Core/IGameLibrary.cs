namespace PS2Home.Core;

/// <summary>
/// Provides access to the PS2 game/application library.
/// </summary>
public interface IGameLibrary
{
    /// <summary>
    /// Returns all games in the library.
    /// </summary>
    Task<IReadOnlyList<GameInfo>> GetAllGamesAsync();
}
