namespace PS2Home.Core;

/// <summary>
/// Sample data implementation of IGameLibrary for development and testing.
/// </summary>
public class SampleGameLibrary : IGameLibrary
{
    private static readonly IReadOnlyList<GameInfo> SampleGames =
    [
        new("Open PS2 Loader (OPL)", "OPL", "Play games from HDD, SMB, or USB", "", true, "Installed"),
        new("FreeMCBoot Installer", "FMCB", "Softmod installer for PS2", "", true, "Installed"),
        new("wLaunchELF", "WLELF", "File manager and ELF launcher", "", false, "Not installed"),
        new("Simple Media System (SMS)", "SMS", "Media player for PS2", "", false, "Not installed"),
        new("PS2 Browser", "PSBROWSER", "Homebrew web browser", "", false, "Not installed"),
        new("Cheat Code Manager (CCAPI)", "CCAPI", "Cheat code management tool", "", false, "Not installed"),
    ];

    public Task<IReadOnlyList<GameInfo>> GetAllGamesAsync()
    {
        return Task.FromResult(SampleGames);
    }
}
