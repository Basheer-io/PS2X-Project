namespace PS2Home.Core;

/// <summary>
/// Filters game lists by search query. Matches against name and tagline (case-insensitive).
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
}
