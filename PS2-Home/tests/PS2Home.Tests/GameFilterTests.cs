using PS2Home.Core;
using Xunit;

namespace PS2Home.Tests;

public class GameFilterTests
{
    private readonly IReadOnlyList<GameInfo> _sampleGames =
    [
        new("Open PS2 Loader (OPL)", "OPL", "Play games from HDD, SMB, or USB", "", true, "Installed"),
        new("FreeMCBoot Installer", "FMCB", "Softmod installer for PS2", "", true, "Installed"),
        new("wLaunchELF", "WLELF", "File manager and ELF launcher", "", false, "Not installed"),
        new("Simple Media System (SMS)", "SMS", "Media player for PS2", "", false, "Not installed"),
        new("PS2 Browser", "PSBROWSER", "Homebrew web browser", "", false, "Not installed"),
        new("Cheat Code Manager (CCAPI)", "CCAPI", "Cheat code management tool", "", false, "Not installed"),
    ];

    [Fact]
    public void Apply_WithNullOrWhitespace_ReturnsAllGames()
    {
        var nullResult = GameFilter.Apply(_sampleGames, null);
        var emptyResult = GameFilter.Apply(_sampleGames, "");
        var whitespaceResult = GameFilter.Apply(_sampleGames, "   ");

        Assert.Equal(_sampleGames.Count, nullResult.Count);
        Assert.Equal(_sampleGames.Count, emptyResult.Count);
        Assert.Equal(_sampleGames.Count, whitespaceResult.Count);
    }

    [Theory]
    [InlineData("OPL", 1)]
    [InlineData("opl", 1)]
    [InlineData("Open PS2", 1)]
    [InlineData("Cheat Code", 1)]
    [InlineData("Browser", 1)]
    public void Apply_FiltersByName_CaseInsensitive(string query, int expectedCount)
    {
        var results = GameFilter.Apply(_sampleGames, query);

        Assert.Equal(expectedCount, results.Count);
    }

    [Theory]
    [InlineData("media", 1, "Simple Media System (SMS)")]
    [InlineData("Softmod", 1, "FreeMCBoot Installer")]
    [InlineData("file manager", 1, "wLaunchELF")]
    [InlineData("HDD", 1, "Open PS2 Loader (OPL)")]
    public void Apply_FiltersByTagline_CaseInsensitive(string query, int expectedCount, string expectedGameName)
    {
        var results = GameFilter.Apply(_sampleGames, query);

        Assert.Equal(expectedCount, results.Count);
        Assert.Equal(expectedGameName, results[0].Name);
    }

    [Fact]
    public void Apply_MatchesEitherNameOrTagline()
    {
        // "PS2" is in the name of OPL and PS2 Browser, and in the tagline of FMCB and SMS
        var results = GameFilter.Apply(_sampleGames, "PS2");

        Assert.Equal(4, results.Count);
    }

    [Fact]
    public void Apply_WithNoMatches_ReturnsEmpty()
    {
        var results = GameFilter.Apply(_sampleGames, "NonExistentApplication12345");

        Assert.Empty(results);
    }
}
