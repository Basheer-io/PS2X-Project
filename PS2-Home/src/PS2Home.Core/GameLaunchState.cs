namespace PS2Home.Core;

/// <summary>
/// Execution state of a launched game.
/// </summary>
public enum GameLaunchState
{
    Idle,
    Starting,
    Running,
    Exited,
    Failed
}
