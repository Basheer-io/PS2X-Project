namespace PS2Home.Core;

/// <summary>
/// Persists and retrieves application configuration.
/// </summary>
public interface ISettingsStore
{
    Task<AppSettings> LoadSettingsAsync(CancellationToken cancellationToken = default);
    Task SaveSettingsAsync(AppSettings settings, CancellationToken cancellationToken = default);
}
