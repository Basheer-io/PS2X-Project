namespace PS2Home.Core;

/// <summary>
/// Persists and retrieves the scanned game library.
/// </summary>
public interface ILibraryStore
{
    Task<IReadOnlyList<GameEntry>> LoadLibraryAsync(CancellationToken cancellationToken = default);
    Task SaveLibraryAsync(IEnumerable<GameEntry> games, CancellationToken cancellationToken = default);
}
