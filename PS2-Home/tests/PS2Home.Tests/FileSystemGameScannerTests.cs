using PS2Home.Core;
using PS2Home.Infrastructure;
using Xunit;

namespace PS2Home.Tests;

public class FileSystemGameScannerTests : IDisposable
{
    private readonly string _testRoot;

    public FileSystemGameScannerTests()
    {
        _testRoot = Path.Combine(Path.GetTempPath(), $"ps2home_scan_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testRoot);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_testRoot))
                Directory.Delete(_testRoot, recursive: true);
        }
        catch
        {
            // Ignore cleanup failures in test teardown
        }
    }

    [Fact]
    public async Task ScanFoldersAsync_EmptyDirectory_ReturnsNoGames()
    {
        var scanner = new FileSystemGameScanner();

        var games = await scanner.ScanFoldersAsync([_testRoot]);

        Assert.Empty(games);
    }

    [Fact]
    public async Task ScanFoldersAsync_FindsIsoFilesRecursively()
    {
        var subDir = Path.Combine(_testRoot, "Action", "RPG");
        Directory.CreateDirectory(subDir);

        var file1 = Path.Combine(_testRoot, "Game_One.iso");
        var file2 = Path.Combine(subDir, "Game_Two.iso");
        var nonGame = Path.Combine(_testRoot, "readme.txt");

        await File.WriteAllTextAsync(file1, "dummy data 1");
        await File.WriteAllTextAsync(file2, "dummy data 2");
        await File.WriteAllTextAsync(nonGame, "not an iso");

        var scanner = new FileSystemGameScanner();

        var games = await scanner.ScanFoldersAsync([_testRoot]);

        Assert.Equal(2, games.Count);
        Assert.Contains(games, g => g.Title == "Game One");
        Assert.Contains(games, g => g.Title == "Game Two");
        Assert.All(games, g => Assert.True(g.FileSizeBytes > 0));
        Assert.All(games, g => Assert.False(g.IsMissing));
    }

    [Fact]
    public async Task ScanFoldersAsync_HandlesSpacesAndUnicodePaths()
    {
        var unicodeDir = Path.Combine(_testRoot, "Gry Kolekcja", "Japonia 日本");
        Directory.CreateDirectory(unicodeDir);

        var filePath = Path.Combine(unicodeDir, "Gra Wideo (PL).iso");
        await File.WriteAllTextAsync(filePath, "dummy");

        var scanner = new FileSystemGameScanner();

        var games = await scanner.ScanFoldersAsync([_testRoot]);

        Assert.Single(games);
        Assert.Equal("Gra Wideo (PL)", games[0].Title);
        Assert.Contains("Japonia 日本", games[0].FullPath);
    }

    [Fact]
    public async Task ScanFoldersAsync_IgnoresDuplicateFoldersAndPaths()
    {
        var filePath = Path.Combine(_testRoot, "Unique_Game.iso");
        await File.WriteAllTextAsync(filePath, "dummy");

        var scanner = new FileSystemGameScanner();

        // Pass the same folder multiple times
        var games = await scanner.ScanFoldersAsync([_testRoot, _testRoot, _testRoot]);

        Assert.Single(games);
    }

    [Fact]
    public async Task ScanFoldersAsync_ReportsProgress()
    {
        var filePath = Path.Combine(_testRoot, "Test_Game.iso");
        await File.WriteAllTextAsync(filePath, "dummy");

        var scanner = new FileSystemGameScanner();
        var progressReports = new List<ScanProgress>();
        var progress = new Progress<ScanProgress>(progressReports.Add);

        await scanner.ScanFoldersAsync([_testRoot], progress);

        // Allow progress callbacks to finish
        await Task.Delay(50);

        Assert.NotEmpty(progressReports);
    }

    [Fact]
    public async Task ScanFoldersAsync_CancelsPromptly()
    {
        var scanner = new FileSystemGameScanner();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            scanner.ScanFoldersAsync([_testRoot], cancellationToken: cts.Token));
    }
}
