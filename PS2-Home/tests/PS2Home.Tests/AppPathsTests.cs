using PS2Home.Core;
using PS2Home.Infrastructure;
using Xunit;

namespace PS2Home.Tests;

public class AppPathsTests
{
    [Fact]
    public void ConfigDirectory_IsInsideProvidedBase()
    {
        var baseDir = Path.Combine(Path.GetTempPath(), "ps2home-test-base");
        var result = AppPaths.GetConfigDirectory(baseDir);

        Assert.Equal(Path.Combine(baseDir, AppInfo.ConfigFolderName), result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ConfigDirectory_RejectsEmptyBase(string baseDir)
    {
        Assert.Throws<ArgumentException>(() => AppPaths.GetConfigDirectory(baseDir));
    }

    [Fact]
    public void ConfigDirectory_HandlesSpacesAndUnicode()
    {
        var baseDir = Path.Combine(Path.GetTempPath(), "mój katalog");
        var result = AppPaths.GetConfigDirectory(baseDir);

        Assert.EndsWith(AppInfo.ConfigFolderName, result);
        Assert.Contains("mój katalog", result);
    }
}
