using PS2Home.Core;
using PS2Home.Infrastructure;
using Xunit;

namespace PS2Home.Tests;

public class SettingsBackupTests : IDisposable
{
    private readonly string _tempDir;
    private readonly JsonSettingsStore _settingsStore;
    private readonly JsonLibraryStore _libraryStore;
    private readonly SettingsBackupService _backupService;

    public SettingsBackupTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "ps2home_backup_tests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);

        _settingsStore = new JsonSettingsStore(Path.Combine(_tempDir, "settings.json"));
        _libraryStore = new JsonLibraryStore(Path.Combine(_tempDir, "library.json"));
        _backupService = new SettingsBackupService(_settingsStore, _libraryStore);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, true);
        }
        catch
        {
            // Ignore test cleanup error
        }
    }

    [Fact]
    public async Task ExportBackup_CreatesValidJsonFileWithMetadata()
    {
        var settings = new AppSettings
        {
            GameFolders = ["/games/ps2"],
            Theme = "Light",
            CouchMode = true
        };
        await _settingsStore.SaveSettingsAsync(settings);

        var games = new List<GameEntry>
        {
            new() { Id = "test-game", Title = "Test Game", FullPath = "/games/ps2/game.iso", FileSizeBytes = 1024 }
        };
        await _libraryStore.SaveLibraryAsync(games);

        var backupPath = Path.Combine(_tempDir, "export_test.json");
        var resultPath = await _backupService.ExportBackupAsync(backupPath);

        Assert.True(File.Exists(resultPath));

        var validation = await _backupService.ValidateBackupAsync(resultPath);
        Assert.True(validation.IsValid);
        Assert.NotNull(validation.Data);
        Assert.Equal(SettingsBackupService.SupportedSchemaVersion, validation.Data.SchemaVersion);
        Assert.Equal("Light", validation.Data.Settings.Theme);
        Assert.True(validation.Data.Settings.CouchMode);
        Assert.Single(validation.Data.Games);
        Assert.Equal("Test Game", validation.Data.Games[0].Title);
    }

    [Fact]
    public async Task ValidateBackup_NonExistentFile_ReturnsInvalid()
    {
        var nonExistent = Path.Combine(_tempDir, "non_existent.json");
        var validation = await _backupService.ValidateBackupAsync(nonExistent);

        Assert.False(validation.IsValid);
        Assert.Contains("does not exist", validation.ErrorMessage);
    }

    [Fact]
    public async Task ValidateBackup_EmptyFile_ReturnsInvalid()
    {
        var emptyFile = Path.Combine(_tempDir, "empty.json");
        await File.WriteAllTextAsync(emptyFile, "");

        var validation = await _backupService.ValidateBackupAsync(emptyFile);
        Assert.False(validation.IsValid);
        Assert.Contains("empty", validation.ErrorMessage);
    }

    [Fact]
    public async Task ValidateBackup_CorruptJson_ReturnsInvalid()
    {
        var corruptFile = Path.Combine(_tempDir, "corrupt.json");
        await File.WriteAllTextAsync(corruptFile, "{ schemaVersion: 1, broken json ");

        var validation = await _backupService.ValidateBackupAsync(corruptFile);
        Assert.False(validation.IsValid);
        Assert.Contains("Invalid JSON", validation.ErrorMessage);
    }

    [Fact]
    public async Task ValidateBackup_UnsupportedFutureSchema_ReturnsInvalid()
    {
        var futureFile = Path.Combine(_tempDir, "future.json");
        var futureJson = """
        {
          "SchemaVersion": 999,
          "ExportedAt": "2026-10-29T00:00:00Z",
          "AppVersion": "9.9.9",
          "Settings": {},
          "Games": []
        }
        """;
        await File.WriteAllTextAsync(futureFile, futureJson);

        var validation = await _backupService.ValidateBackupAsync(futureFile);
        Assert.False(validation.IsValid);
        Assert.Contains("Unsupported backup schema version", validation.ErrorMessage);
    }

    [Fact]
    public async Task RestoreBackup_OverwritesSettingsAndLibrarySuccessfully()
    {
        var backupPath = Path.Combine(_tempDir, "restore_test.json");
        var validJson = """
        {
          "SchemaVersion": 1,
          "ExportedAt": "2026-10-29T10:00:00Z",
          "AppVersion": "1.0.0",
          "Settings": {
            "GameFolders": ["/imported/folder"],
            "Theme": "Dark",
            "CouchMode": true
          },
          "Games": [
            {
              "Id": "imported-game",
              "Title": "Restored Title",
              "FullPath": "/imported/folder/game.iso",
              "FileSizeBytes": 2048,
              "IsMissing": false
            }
          ]
        }
        """;
        await File.WriteAllTextAsync(backupPath, validJson);

        var restored = await _backupService.RestoreBackupAsync(backupPath);
        Assert.True(restored);

        var loadedSettings = await _settingsStore.LoadSettingsAsync();
        Assert.Equal(["/imported/folder"], loadedSettings.GameFolders);
        Assert.True(loadedSettings.CouchMode);

        var loadedGames = await _libraryStore.LoadLibraryAsync();
        Assert.Single(loadedGames);
        Assert.Equal("Restored Title", loadedGames[0].Title);
    }
}
