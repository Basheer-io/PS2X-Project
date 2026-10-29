using System.Text.Json;
using PS2Home.Core;

namespace PS2Home.Infrastructure;

/// <summary>
/// Exports and imports full application settings and library metadata with validation.
/// </summary>
public class SettingsBackupService : ISettingsBackupService
{
    public const int SupportedSchemaVersion = 1;
    public const long MaxBackupSizeBytes = 50 * 1024 * 1024; // 50 MB

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private readonly ISettingsStore _settingsStore;
    private readonly ILibraryStore _libraryStore;

    public SettingsBackupService(ISettingsStore settingsStore, ILibraryStore libraryStore)
    {
        _settingsStore = settingsStore;
        _libraryStore = libraryStore;
    }

    public async Task<string> ExportBackupAsync(string targetFilePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(targetFilePath))
            throw new ArgumentException("Target file path must not be empty.", nameof(targetFilePath));

        var settings = await _settingsStore.LoadSettingsAsync(cancellationToken);
        var games = await _libraryStore.LoadLibraryAsync(cancellationToken);

        var backup = new BackupData
        {
            SchemaVersion = SupportedSchemaVersion,
            ExportedAt = DateTimeOffset.UtcNow,
            AppVersion = AppInfo.Version,
            Settings = settings,
            Games = [.. games]
        };

        var dir = Path.GetDirectoryName(Path.GetFullPath(targetFilePath));
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        var tempPath = $"{targetFilePath}.{Guid.NewGuid():N}.tmp";
        await using (var stream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
        {
            await JsonSerializer.SerializeAsync(stream, backup, JsonOpts, cancellationToken);
        }

        File.Move(tempPath, targetFilePath, overwrite: true);
        return targetFilePath;
    }

    public async Task<BackupValidationResult> ValidateBackupAsync(string sourceFilePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sourceFilePath))
            return new BackupValidationResult(false, "Source file path is empty.");

        if (!File.Exists(sourceFilePath))
            return new BackupValidationResult(false, $"Backup file does not exist at '{sourceFilePath}'.");

        try
        {
            var fileInfo = new FileInfo(sourceFilePath);
            if (fileInfo.Length == 0)
                return new BackupValidationResult(false, "Backup file is empty (0 bytes).");

            if (fileInfo.Length > MaxBackupSizeBytes)
                return new BackupValidationResult(false, $"Backup file exceeds maximum allowed size ({fileInfo.Length} bytes).");

            await using var stream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
            var backup = await JsonSerializer.DeserializeAsync<BackupData>(stream, JsonOpts, cancellationToken);

            if (backup == null)
                return new BackupValidationResult(false, "Failed to parse backup data: result was null.");

            if (backup.SchemaVersion <= 0)
                return new BackupValidationResult(false, $"Invalid schema version {backup.SchemaVersion}. Must be 1 or higher.");

            if (backup.SchemaVersion > SupportedSchemaVersion)
                return new BackupValidationResult(false, $"Unsupported backup schema version {backup.SchemaVersion} (highest supported is {SupportedSchemaVersion}). Please update PS2 Home.");

            if (backup.Settings == null)
                return new BackupValidationResult(false, "Backup is missing the Settings object.");

            if (backup.Games == null)
                return new BackupValidationResult(false, "Backup is missing the Games collection.");

            return new BackupValidationResult(true, null, backup);
        }
        catch (JsonException jex)
        {
            return new BackupValidationResult(false, $"Invalid JSON syntax: {jex.Message}");
        }
        catch (Exception ex)
        {
            return new BackupValidationResult(false, $"Error reading backup: {ex.Message}");
        }
    }

    public async Task<bool> RestoreBackupAsync(string sourceFilePath, CancellationToken cancellationToken = default)
    {
        var validation = await ValidateBackupAsync(sourceFilePath, cancellationToken);
        if (!validation.IsValid || validation.Data == null)
            throw new InvalidOperationException($"Cannot restore invalid backup: {validation.ErrorMessage}");

        await _settingsStore.SaveSettingsAsync(validation.Data.Settings, cancellationToken);
        await _libraryStore.SaveLibraryAsync(validation.Data.Games, cancellationToken);
        return true;
    }
}
