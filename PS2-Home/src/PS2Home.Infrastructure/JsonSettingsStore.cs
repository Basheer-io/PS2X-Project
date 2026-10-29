using System.Text.Json;
using PS2Home.Core;

namespace PS2Home.Infrastructure;

/// <summary>
/// Persists and loads application settings to/from JSON with atomic file writes
/// and automated backup upon encountering corrupt JSON.
/// </summary>
public class JsonSettingsStore : ISettingsStore
{
    private readonly string _settingsFilePath;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public JsonSettingsStore() : this(AppPaths.GetConfigDirectory())
    {
    }

    public JsonSettingsStore(string configDirectory)
    {
        Directory.CreateDirectory(configDirectory);
        _settingsFilePath = Path.Combine(configDirectory, "settings.json");
    }

    public async Task<AppSettings> LoadSettingsAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_settingsFilePath))
        {
            return new AppSettings();
        }

        try
        {
            var json = await File.ReadAllTextAsync(_settingsFilePath, cancellationToken);
            if (string.IsNullOrWhiteSpace(json))
                return new AppSettings();

            var settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);
            return settings ?? new AppSettings();
        }
        catch (JsonException)
        {
            BackupCorruptedFile();
            return new AppSettings();
        }
    }

    public async Task SaveSettingsAsync(AppSettings settings, CancellationToken cancellationToken = default)
    {
        var tempFilePath = $"{_settingsFilePath}.tmp.{Guid.NewGuid():N}";
        try
        {
            var json = JsonSerializer.Serialize(settings, JsonOptions);
            await File.WriteAllTextAsync(tempFilePath, json, cancellationToken);

            File.Move(tempFilePath, _settingsFilePath, overwrite: true);
        }
        finally
        {
            if (File.Exists(tempFilePath))
            {
                try { File.Delete(tempFilePath); } catch { }
            }
        }
    }

    private void BackupCorruptedFile()
    {
        try
        {
            var backupPath = $"{_settingsFilePath}.corrupt.{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}.bak";
            File.Copy(_settingsFilePath, backupPath, overwrite: true);
        }
        catch
        {
            // Ignore backup failures
        }
    }
}
