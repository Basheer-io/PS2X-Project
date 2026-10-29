using System.Runtime.InteropServices;
using PS2Home.Core;
using PS2Home.Infrastructure;
using Xunit;

namespace PS2Home.Tests;

public class Pcsx2LocatorTests : IDisposable
{
    private readonly string _tempDir;

    public Pcsx2LocatorTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"ps2home_locator_test_{Guid.NewGuid():N}");
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
            // Ignore teardown errors
        }
    }

    private static void MakeExecutable(string filePath)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            try
            {
                File.SetUnixFileMode(filePath,
                    UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute |
                    UnixFileMode.GroupRead | UnixFileMode.GroupExecute |
                    UnixFileMode.OtherRead | UnixFileMode.OtherExecute);
            }
            catch
            {
                // Ignore if platform does not support chmod
            }
        }
    }

    [Fact]
    public async Task ValidateInstallationAsync_WhenFileDoesNotExist_ReturnsInvalid()
    {
        var runner = new FakeProcessRunner();
        var locator = new Pcsx2Locator(runner);

        var nonExistentPath = Path.Combine(_tempDir, "does_not_exist.bin");
        var install = Pcsx2Installation.CreateExecutable(nonExistentPath);

        var result = await locator.ValidateInstallationAsync(install);

        Assert.False(result.IsValid);
        Assert.Contains("not found", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(runner.ExecutedCalls);
    }

    [Fact]
    public async Task ValidateInstallationAsync_WhenFileIsNotExecutable_RejectsWithExplanation()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return; // Unix permission test only

        var nonExecFile = Path.Combine(_tempDir, "pcsx2_not_exec.bin");
        await File.WriteAllTextAsync(nonExecFile, "dummy");
        File.SetUnixFileMode(nonExecFile, UnixFileMode.UserRead | UnixFileMode.UserWrite);

        var runner = new FakeProcessRunner();
        var locator = new Pcsx2Locator(runner);

        var install = Pcsx2Installation.CreateExecutable(nonExecFile);
        var result = await locator.ValidateInstallationAsync(install);

        Assert.False(result.IsValid);
        Assert.Contains("executable", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(runner.ExecutedCalls);
    }

    [Fact]
    public async Task ValidateInstallationAsync_WhenProcessTimesOut_ReturnsInvalidWithTimeoutMessage()
    {
        var dummyFile = Path.Combine(_tempDir, "pcsx2.bin");
        await File.WriteAllTextAsync(dummyFile, "dummy");
        MakeExecutable(dummyFile);

        var runner = new FakeProcessRunner
        {
            Handler = (_, _) => new ProcessExecutionResult(-1, string.Empty, "Timed out", true)
        };
        var locator = new Pcsx2Locator(runner);

        var install = Pcsx2Installation.CreateExecutable(dummyFile);
        var result = await locator.ValidateInstallationAsync(install);

        Assert.False(result.IsValid);
        Assert.Contains("timed out", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ValidateInstallationAsync_WhenExecutionSucceeds_ExtractsVersion()
    {
        var dummyFile = Path.Combine(_tempDir, "pcsx2-qt");
        await File.WriteAllTextAsync(dummyFile, "dummy");
        MakeExecutable(dummyFile);

        var runner = new FakeProcessRunner
        {
            Handler = (_, _) => new ProcessExecutionResult(0, "PCSX2 v2.1.154 (git)", string.Empty, false)
        };
        var locator = new Pcsx2Locator(runner);

        var install = Pcsx2Installation.CreateExecutable(dummyFile);
        var result = await locator.ValidateInstallationAsync(install);

        Assert.True(result.IsValid);
        Assert.Equal("v2.1.154", result.Version);
    }

    [Fact]
    public async Task ValidateInstallationAsync_FlatpakSuccess_ExtractsVersion()
    {
        var runner = new FakeProcessRunner
        {
            Handler = (bin, args) =>
            {
                if (bin == "flatpak" && args.Contains("net.pcsx2.PCSX2"))
                {
                    return new ProcessExecutionResult(0, "PCSX2 - PlayStation 2 Emulator\nVersion: 2.1.0\nBranch: stable", string.Empty, false);
                }
                return new ProcessExecutionResult(1, string.Empty, "Not found", false);
            }
        };
        var locator = new Pcsx2Locator(runner);

        var install = Pcsx2Installation.CreateFlatpak();
        var result = await locator.ValidateInstallationAsync(install);

        Assert.True(result.IsValid);
        Assert.Equal("2.1.0", result.Version);
    }

    [Fact]
    public async Task ValidateInstallationAsync_HandlesSpacesAndUnicodePaths()
    {
        var subDir = Path.Combine(_tempDir, "Katalog Użytkownika 🎮");
        Directory.CreateDirectory(subDir);

        var dummyFile = Path.Combine(subDir, "emulator pcsx2.bin");
        await File.WriteAllTextAsync(dummyFile, "dummy");
        MakeExecutable(dummyFile);

        var runner = new FakeProcessRunner
        {
            Handler = (file, args) =>
            {
                Assert.Contains("Katalog Użytkownika 🎮", file);
                return new ProcessExecutionResult(0, "PCSX2 2.2.0-nightly", string.Empty, false);
            }
        };
        var locator = new Pcsx2Locator(runner);

        var install = Pcsx2Installation.CreateExecutable(dummyFile);
        var result = await locator.ValidateInstallationAsync(install);

        Assert.True(result.IsValid);
        Assert.Equal("2.2.0-nightly", result.Version);
    }
}
