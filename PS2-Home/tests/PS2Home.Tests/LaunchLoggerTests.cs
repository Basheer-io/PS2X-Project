using PS2Home.Core;
using PS2Home.Infrastructure;
using Xunit;

namespace PS2Home.Tests;

public class LaunchLoggerTests : IDisposable
{
    private readonly string _testConfigDir;

    public LaunchLoggerTests()
    {
        _testConfigDir = Path.Combine(Path.GetTempPath(), $"ps2home_logger_test_{Guid.NewGuid():N}");
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
            // Ignore teardown cleanup errors
        }
    }

    [Fact]
    public void RedactHomePath_ReplacesHomeDirectoryWithTilde()
    {
        var logger = new LaunchLogger(_testConfigDir);
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        var pathWithHome = Path.Combine(home, "Games", "game.iso");
        var redacted = logger.RedactHomePath(pathWithHome);

        Assert.DoesNotContain(home, redacted);
        Assert.StartsWith("~", redacted);
    }

    [Fact]
    public async Task LogLaunchStarted_And_Finished_WritesToFile()
    {
        var logger = new LaunchLogger(_testConfigDir);

        await logger.LogLaunchStartedAsync(
            "/usr/bin/pcsx2",
            ["-batch", "-fullscreen", "--", "/games/test.iso"],
            "/games/test.iso");

        var sessionResult = new GameSessionResult(
            ExitCode: 0,
            Duration: TimeSpan.FromMinutes(5),
            ErrorSummary: null,
            FastFailure: false);

        await logger.LogLaunchFinishedAsync("/games/test.iso", sessionResult);

        var logs = await logger.GetRecentLogsAsync(10);

        Assert.Equal(2, logs.Count);
        Assert.Contains("[START]", logs[0]);
        Assert.Contains("[EXITED]", logs[1]);
        Assert.Contains("ExitCode: 0", logs[1]);
    }

    [Fact]
    public async Task LogLaunchFinished_FastFailure_LoggedWithStatusAndError()
    {
        var logger = new LaunchLogger(_testConfigDir);

        var sessionResult = new GameSessionResult(
            ExitCode: 1,
            Duration: TimeSpan.FromSeconds(1),
            ErrorSummary: "Failed to open CDVD image",
            FastFailure: true);

        await logger.LogLaunchFinishedAsync("/games/corrupt.iso", sessionResult);

        var logs = await logger.GetRecentLogsAsync(10);

        Assert.Single(logs);
        Assert.Contains("[FAILED_FAST]", logs[0]);
        Assert.Contains("Failed to open CDVD image", logs[0]);
    }

    [Fact]
    public async Task ExportRedactedLogsAsync_ReturnsContent()
    {
        var logger = new LaunchLogger(_testConfigDir);

        await logger.LogLaunchStartedAsync("flatpak", ["run", "net.pcsx2.PCSX2"], "/games/sample.iso");

        var exported = await logger.ExportRedactedLogsAsync();

        Assert.NotNull(exported);
        Assert.Contains("sample.iso", exported);
    }
}
