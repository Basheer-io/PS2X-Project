using PS2Home.Core;
using Xunit;

namespace PS2Home.Tests;

public class Pcsx2ArgumentBuilderTests
{
    private readonly Pcsx2Installation _nativeInstall = Pcsx2Installation.CreateExecutable("/usr/bin/pcsx2-qt");
    private readonly Pcsx2Installation _flatpakInstall = Pcsx2Installation.CreateFlatpak();

    [Fact]
    public void BuildArguments_DefaultOptions_IncludesBatchFullscreenAndSeparator()
    {
        var request = new LaunchRequest
        {
            Installation = _nativeInstall,
            GamePath = "/games/ps2/final_fantasy.iso"
        };

        var args = Pcsx2ArgumentBuilder.BuildArguments(request);

        Assert.Contains("-batch", args);
        Assert.Contains("-fullscreen", args);
        Assert.DoesNotContain("-nogui", args);

        // Verify '--' precedes the game path
        var separatorIndex = args.ToList().IndexOf("--");
        var gameIndex = args.ToList().IndexOf("/games/ps2/final_fantasy.iso");

        Assert.True(separatorIndex >= 0);
        Assert.Equal(separatorIndex + 1, gameIndex);
    }

    [Fact]
    public void BuildArguments_WithFlatpak_IncludesLeadingArgsBeforeOptions()
    {
        var request = new LaunchRequest
        {
            Installation = _flatpakInstall,
            GamePath = "/games/ps2/game.iso"
        };

        var args = Pcsx2ArgumentBuilder.BuildArguments(request);

        Assert.Equal("run", args[0]);
        Assert.Equal("net.pcsx2.PCSX2", args[1]);
        Assert.Contains("-batch", args);
        Assert.Contains("--", args);
        Assert.Equal("/games/ps2/game.iso", args.Last());
    }

    [Fact]
    public void BuildArguments_DisabledOptions_OmitsFlags()
    {
        var request = new LaunchRequest
        {
            Installation = _nativeInstall,
            GamePath = "/games/ps2/test.iso",
            Fullscreen = false,
            BatchMode = false,
            NoGui = true
        };

        var args = Pcsx2ArgumentBuilder.BuildArguments(request);

        Assert.DoesNotContain("-fullscreen", args);
        Assert.DoesNotContain("-batch", args);
        Assert.Contains("-nogui", args);
        Assert.Equal("--", args[^2]);
        Assert.Equal("/games/ps2/test.iso", args[^1]);
    }

    [Theory]
    [InlineData("-dash_in_name.iso")]
    [InlineData("--double-dash.iso")]
    public void BuildArguments_LeadingDashesInFilename_ProtectedBySeparator(string filename)
    {
        var fullPath = $"/games/{filename}";
        var request = new LaunchRequest
        {
            Installation = _nativeInstall,
            GamePath = fullPath
        };

        var args = Pcsx2ArgumentBuilder.BuildArguments(request);

        var separatorIndex = args.ToList().IndexOf("--");
        Assert.True(separatorIndex >= 0);
        Assert.Equal(fullPath, args[separatorIndex + 1]);
    }

    [Fact]
    public void BuildArguments_SpacesAndUnicodeInPath_PreservedAsSingleArgument()
    {
        const string specialPath = "/Gry Użytkownika 🎮/Final Fantasy X \"Special Edition\".iso";
        var request = new LaunchRequest
        {
            Installation = _nativeInstall,
            GamePath = specialPath
        };

        var args = Pcsx2ArgumentBuilder.BuildArguments(request);

        Assert.Equal(specialPath, args.Last());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void BuildArguments_EmptyGamePath_ThrowsArgumentException(string? invalidPath)
    {
        var request = new LaunchRequest
        {
            Installation = _nativeInstall,
            GamePath = invalidPath!
        };

        Assert.Throws<ArgumentException>(() => Pcsx2ArgumentBuilder.BuildArguments(request));
    }
}
