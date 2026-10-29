namespace PS2Home.Core;

/// <summary>
/// Filters games and applications by search query. Matches against title and tags/path (case-insensitive).
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
}
