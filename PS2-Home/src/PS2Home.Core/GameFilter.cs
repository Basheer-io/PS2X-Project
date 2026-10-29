namespace PS2Home.Core;

/// <summary>
/// Filters games and applications by search query and recents ordering. Matches against title and tags/path (case-insensitive).
/// </summary>
public static class GameFilter
{
    public static IReadOnlyList<GameInfo> Apply(IEnumerable<GameInfo> games, string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return games as IReadOnlyList<GameInfo> ?? games.ToList();

        return games
            .Where(g =>
                g.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                g.Tagline.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public static IReadOnlyList<GameEntry> Apply(IEnumerable<GameEntry> games, string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return games as IReadOnlyList<GameEntry> ?? games.ToList();

        return games
            .Where(g =>
                g.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                g.FullPath.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    /// Filters and orders games by most recently played. Excludes unplayed or missing games.
    /// </summary>
    public static IReadOnlyList<GameEntry> GetRecentlyPlayed(IEnumerable<GameEntry> games, int maxCount = 5)
    {
        return games
            .Where(g => g.LastPlayedAt.HasValue && !g.IsMissing)
            .OrderByDescending(g => g.LastPlayedAt!.Value)
            .Take(maxCount)
            .ToList();
    }
}
