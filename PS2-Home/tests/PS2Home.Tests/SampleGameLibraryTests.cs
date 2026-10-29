using PS2Home.Core;
using Xunit;

namespace PS2Home.Tests;

public class SampleGameLibraryTests
{
    [Fact]
    public async Task GetAllGamesAsync_ReturnsSampleGames()
    {
        var library = new SampleGameLibrary();

        var games = await library.GetAllGamesAsync();

        Assert.NotNull(games);
        Assert.NotEmpty(games);
        Assert.Contains(games, g => g.Name.Contains("OPL"));
        Assert.Contains(games, g => g.IsInstalled);
        Assert.Contains(games, g => !g.IsInstalled);
    }
}
