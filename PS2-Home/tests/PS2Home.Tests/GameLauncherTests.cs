using PS2Home.Core;
using PS2Home.Infrastructure;
using Xunit;

namespace PS2Home.Tests;

public class GameLauncherTests : IDisposable
{
    private readonly string _tempDir;

    public GameLauncherTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"ps2home_launcher_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, recursive: true);
        }
        catch
        {
            // Ignore teardown cleanup errors
        }
    }

    [Fact]
    public async Task LaunchGameAsync_WhenFileDoesNotExist_ThrowsFileNotFoundException()
    {
        var logger = new LaunchLogger(_tempDir);
        var launcher = new GameLauncher(logger);

        var request = new LaunchRequest
        {
            Installation = Pcsx2Installation.CreateExecutable("/usr/bin/pcsx2"),
            GamePath = Path.Combine(_tempDir, "non_existent.iso")
        };

        await Assert.ThrowsAsync<FileNotFoundException>(() =>
            launcher.LaunchGameAsync(request));
    }
}
