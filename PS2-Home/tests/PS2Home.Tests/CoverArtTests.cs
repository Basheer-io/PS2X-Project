using PS2Home.Core;
using PS2Home.Infrastructure;
using Xunit;

namespace PS2Home.Tests;

public class CoverArtTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _coversDir;
    private readonly CoverArtManager _coverArtManager;

    public CoverArtTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "ps2home_cover_tests_" + Guid.NewGuid().ToString("N"));
        _coversDir = Path.Combine(_tempDir, "covers");
        Directory.CreateDirectory(_tempDir);
        _coverArtManager = new CoverArtManager(_coversDir);
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

    [Theory]
    [InlineData(".png")]
    [InlineData(".jpg")]
    [InlineData(".jpeg")]
    [InlineData(".webp")]
    [InlineData(".bmp")]
    public async Task IsValidImageFile_AllowsSupportedExtensions(string ext)
    {
        var filePath = Path.Combine(_tempDir, $"sample{ext}");
        await File.WriteAllBytesAsync(filePath, [0x89, 0x50, 0x4E, 0x47]); // fake header bytes

        var valid = _coverArtManager.IsValidImageFile(filePath, out var err);
        Assert.True(valid, err);
        Assert.Null(err);
    }

    [Theory]
    [InlineData(".exe")]
    [InlineData(".iso")]
    [InlineData(".bin")]
    [InlineData(".txt")]
    [InlineData(".dll")]
    public async Task IsValidImageFile_RejectsUnsupportedExtensions(string ext)
    {
        var filePath = Path.Combine(_tempDir, $"bad{ext}");
        await File.WriteAllBytesAsync(filePath, [0x01, 0x02]);

        var valid = _coverArtManager.IsValidImageFile(filePath, out var err);
        Assert.False(valid);
        Assert.Contains("Unsupported image format", err);
    }

    [Fact]
    public async Task IsValidImageFile_RejectsEmptyFile()
    {
        var filePath = Path.Combine(_tempDir, "empty.png");
        await File.WriteAllBytesAsync(filePath, []);

        var valid = _coverArtManager.IsValidImageFile(filePath, out var err);
        Assert.False(valid);
        Assert.Contains("empty", err);
    }

    [Fact]
    public async Task SetCoverArt_CopiesImageToCoversDirectory()
    {
        var sourceImage = Path.Combine(_tempDir, "original.png");
        await File.WriteAllBytesAsync(sourceImage, [0x01, 0x02, 0x03]);

        var gameId = "Gran Turismo 4 (USA)";
        var destPath = await _coverArtManager.SetCoverArtAsync(gameId, sourceImage);

        Assert.NotNull(destPath);
        Assert.True(File.Exists(destPath));
        Assert.StartsWith(_coversDir, destPath);

        // Verify content preserved
        var bytes = await File.ReadAllBytesAsync(destPath);
        Assert.Equal([0x01, 0x02, 0x03], bytes);
    }

    [Fact]
    public void ResolveCoverArtPath_ReturnsNull_WhenMissingOrDeleted()
    {
        var gameId = "NonExistentGame";
        var resolved = _coverArtManager.ResolveCoverArtPath(gameId, "/non/existent/path/cover.png");

        Assert.Null(resolved);
    }

    [Fact]
    public async Task RemoveCoverArt_DeletesFileSuccessfully()
    {
        var sourceImage = Path.Combine(_tempDir, "test.png");
        await File.WriteAllBytesAsync(sourceImage, [0xAA, 0xBB]);

        var gameId = "GameToRemove";
        var destPath = await _coverArtManager.SetCoverArtAsync(gameId, sourceImage);
        Assert.True(File.Exists(destPath));

        var removed = await _coverArtManager.RemoveCoverArtAsync(gameId);
        Assert.True(removed);
        Assert.False(File.Exists(destPath));
    }
}
