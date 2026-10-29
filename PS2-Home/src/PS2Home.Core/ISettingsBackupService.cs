namespace PS2Home.Core;

/// <summary>
/// Data payload for exported settings and library backups.
/// </summary>
public record BackupData
{
    /// <summary>
    /// Schema format version.
    /// </summary>
    public int SchemaVersion { get; init; } = 1;

    /// <summary>
    /// Timestamp when backup was generated.
    /// </summary>
    public DateTimeOffset ExportedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Version of PS2 Home that exported this backup.
    /// </summary>
    public string AppVersion { get; init; } = AppInfo.Version;

    /// <summary>
    /// Application settings.
    /// </summary>
    public AppSettings Settings { get; init; } = new();

    /// <summary>
    /// Game library entries.
    /// </summary>
    public List<GameEntry> Games { get; init; } = [];
}

/// <summary>
/// Result of validating a backup JSON file.
/// </summary>
public record BackupValidationResult(
    bool IsValid,
    string? ErrorMessage = null,
    BackupData? Data = null);

/// <summary>
/// Exports and imports application settings and game library backups with validation.
/// </summary>
public interface ISettingsBackupService
{
    /// <summary>
    /// Exports current settings and library to a JSON file at targetFilePath.
    /// </summary>
    Task<string> ExportBackupAsync(string targetFilePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates an existing backup file without applying it.
    /// </summary>
    Task<BackupValidationResult> ValidateBackupAsync(string sourceFilePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates and restores settings and library from a backup JSON file.
    /// </summary>
    Task<bool> RestoreBackupAsync(string sourceFilePath, CancellationToken cancellationToken = default);
}
