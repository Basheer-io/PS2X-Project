using PS2Home.Core;
using Xunit;

namespace PS2Home.Tests;

public class RecentsOrderingTests
{
    [Fact]
    public void GetRecentlyPlayed_OrdersByDescendingPlayedTimestamp()
    {
        var now = DateTimeOffset.UtcNow;
        var games = new List<GameEntry>
        {
            new() { Id = "game1", Title = "Game 1", FullPath = "/games/g1.iso", LastPlayedAt = now.AddHours(-2) },
            new() { Id = "game2", Title = "Game 2", FullPath = "/games/g2.iso", LastPlayedAt = now.AddMinutes(-10) }, // Most recent
            new() { Id = "game3", Title = "Game 3", FullPath = "/games/g3.iso", LastPlayedAt = now.AddDays(-1) },
            new() { Id = "game4", Title = "Game 4", FullPath = "/games/g4.iso", LastPlayedAt = null } // Never played
        };

        var recents = GameFilter.GetRecentlyPlayed(games);

        Assert.Equal(3, recents.Count);
        Assert.Equal("Game 2", recents[0].Title);
        Assert.Equal("Game 1", recents[1].Title);
        Assert.Equal("Game 3", recents[2].Title);
    }

    [Fact]
    public void GetRecentlyPlayed_ExcludesUnplayedOrMissingGames()
    {
        var now = DateTimeOffset.UtcNow;
        var games = new List<GameEntry>
        {
            new() { Id = "game1", Title = "Unplayed", FullPath = "/games/u1.iso", LastPlayedAt = null },
            new() { Id = "game2", Title = "Missing", FullPath = "/games/m1.iso", LastPlayedAt = now.AddMinutes(-5), IsMissing = true }
        };

        var recents = GameFilter.GetRecentlyPlayed(games);

        Assert.Empty(recents);
    }

    [Fact]
    public void GetRecentlyPlayed_CapsAtSpecifiedCount()
    {
        var now = DateTimeOffset.UtcNow;
        var games = Enumerable.Range(1, 10).Select(i => new GameEntry
        {
            Id = $"game{i}",
            Title = $"Game {i}",
            FullPath = $"/games/g{i}.iso",
            LastPlayedAt = now.AddMinutes(-i)
        }).ToList();

        var recents = GameFilter.GetRecentlyPlayed(games, maxCount: 5);

        Assert.Equal(5, recents.Count);
        Assert.Equal("Game 1", recents[0].Title);
        Assert.Equal("Game 5", recents[4].Title);
    }
}
