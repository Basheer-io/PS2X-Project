using PS2Home.Core;
using PS2Home.Infrastructure;
using Xunit;

namespace PS2Home.Tests;

public class JsonPersistenceTests : IDisposable
{
    private readonly string _testConfigDir;

    public JsonPersistenceTests()
    {
        _testConfigDir = Path.Combine(Path.GetTempPath(), $"ps2home_json_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testConfigDir);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_testConfigDir))
                Directory.Delete(_testConfigDir, recursive: true);
        }
        catch
        {
            // Ignore teardown failures
        }
    }

    [Fact]
    public async Task SettingsStore_SaveAndLoad_RoundTrip()
    {
        var store = new JsonSettingsStore(_testConfigDir);

        var original = new AppSettings
        {
            GameFolders = ["/games/ps2", "/storage/isos"],
            CoverArtFolder = "/games/covers",
            AutoScanOnStartup = false,
            ShowHiddenApps = true,
            Theme = "Light"
        };

        await store.SaveSettingsAsync(original);
        var loaded = await store.LoadSettingsAsync();

        Assert.Equal(original.GameFolders, loaded.GameFolders);
        Assert.Equal(original.CoverArtFolder, loaded.CoverArtFolder);
        Assert.Equal(original.AutoScanOnStartup, loaded.AutoScanOnStartup);
        Assert.Equal(original.ShowHiddenApps, loaded.ShowHiddenApps);
        Assert.Equal(original.Theme, loaded.Theme);
    }

    [Fact]
    public async Task SettingsStore_MissingFile_ReturnsDefault()
    {
        var store = new JsonSettingsStore(_testConfigDir);

        var settings = await store.LoadSettingsAsync();

        Assert.NotNull(settings);
        Assert.Empty(settings.GameFolders);
        Assert.True(settings.AutoScanOnStartup);
    }

    [Fact]
    public async Task SettingsStore_CorruptedJson_RecoversWithBackup()
    {
        var settingsFile = Path.Combine(_testConfigDir, "settings.json");
        await File.WriteAllTextAsync(settingsFile, "{ this is invalid JSON !!! }");

        var store = new JsonSettingsStore(_testConfigDir);
        var settings = await store.LoadSettingsAsync();

        // Must not crash and return defaults
        Assert.NotNull(settings);

        // Verify a .bak backup file was created
        var backupFiles = Directory.GetFiles(_testConfigDir, "*.corrupt.*.bak");
        Assert.NotEmpty(backupFiles);
    }

    [Fact]
    public async Task LibraryStore_SaveAndLoad_RoundTrip()
    {
        var store = new JsonLibraryStore(_testConfigDir);

        var games = new List<GameEntry>
        {
            new()
            {
                Id = "/games/game1.iso",
                Title = "Game 1",
                FullPath = "/games/game1.iso",
                FileSizeBytes = 4000000000,
                AddedAt = DateTimeOffset.UtcNow,
                LastScannedAt = DateTimeOffset.UtcNow,
                IsMissing = false
            },
            new()
            {
                Id = "/games/game2.iso",
                Title = "Game 2",
                FullPath = "/games/game2.iso",
                FileSizeBytes = 2000000000,
                AddedAt = DateTimeOffset.UtcNow,
                LastScannedAt = DateTimeOffset.UtcNow,
                IsMissing = true
            }
        };

        await store.SaveLibraryAsync(games);
        var loaded = await store.LoadLibraryAsync();

        Assert.Equal(2, loaded.Count);
        Assert.Equal(games[0].Title, loaded[0].Title);
        Assert.Equal(games[1].IsMissing, loaded[1].IsMissing);
    }

    [Fact]
    public async Task LibraryStore_CorruptedJson_RecoversWithBackup()
    {
        var libFile = Path.Combine(_testConfigDir, "library.json");
        await File.WriteAllTextAsync(libFile, "--- broken not json ---");

        var store = new JsonLibraryStore(_testConfigDir);
        var games = await store.LoadLibraryAsync();

        Assert.Empty(games);

        var backupFiles = Directory.GetFiles(_testConfigDir, "*.corrupt.*.bak");
        Assert.NotEmpty(backupFiles);
    }
}
