using PS2Home.Core;
using PS2Home.Infrastructure;
using Xunit;

namespace PS2Home.Tests;

public class RealGameLibraryTests : IDisposable
{
    private readonly string _testDir;
    private readonly string _testGamesDir;

    public RealGameLibraryTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), $"ps2home_lib_test_{Guid.NewGuid():N}");
        _testGamesDir = Path.Combine(_testDir, "games");
        Directory.CreateDirectory(_testGamesDir);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_testDir))
                Directory.Delete(_testDir, recursive: true);
        }
        catch
        {
            // Ignore teardown failures
        }
    }

    [Fact]
    public async Task RescanAsync_FindsGames_AndPersistsToStore()
    {
        var gameFile = Path.Combine(_testGamesDir, "Gran_Turismo_4.iso");
        await File.WriteAllTextAsync(gameFile, "dummy disc");

        var store = new JsonLibraryStore(_testDir);
        var scanner = new FileSystemGameScanner();
        var library = new RealGameLibrary(store, scanner);

        var scanned = await library.RescanAsync([_testGamesDir]);

        Assert.Single(scanned);
        Assert.Equal("Gran Turismo 4", scanned[0].Title);
        Assert.False(scanned[0].IsMissing);

        // Verify persisted
        var loaded = await store.LoadLibraryAsync();
        Assert.Single(loaded);
        Assert.Equal("Gran Turismo 4", loaded[0].Title);
    }

    [Fact]
    public async Task RescanAsync_WhenFileDeleted_FlagsAsMissing()
    {
        var gameFile = Path.Combine(_testGamesDir, "Tekken_5.iso");
        await File.WriteAllTextAsync(gameFile, "dummy");

        var store = new JsonLibraryStore(_testDir);
        var scanner = new FileSystemGameScanner();
        var library = new RealGameLibrary(store, scanner);

        // Initial scan
        await library.RescanAsync([_testGamesDir]);

        // Delete the file
        File.Delete(gameFile);

        // Rescan another (empty) folder or the same folder
        var reScanned = await library.RescanAsync([_testGamesDir]);

        Assert.Single(reScanned);
        Assert.True(reScanned[0].IsMissing);
    }
}
