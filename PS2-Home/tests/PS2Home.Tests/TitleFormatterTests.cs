using PS2Home.Core;
using Xunit;

namespace PS2Home.Tests;

public class TitleFormatterTests
{
    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("   ", "")]
    public void Format_EmptyOrWhitespace_ReturnsEmpty(string? input, string expected)
    {
        Assert.Equal(expected, TitleFormatter.Format(input));
    }

    [Theory]
    [InlineData("Gran_Turismo_4.iso", "Gran Turismo 4")]
    [InlineData("Final_Fantasy_X_International.iso", "Final Fantasy X International")]
    [InlineData("Metal_Gear_Solid_3_Snake_Eater.iso", "Metal Gear Solid 3 Snake Eater")]
    [InlineData("Shadow_of_the_Colossus.iso", "Shadow of the Colossus")]
    public void Format_ReplacesUnderscoresWithSpaces(string input, string expected)
    {
        Assert.Equal(expected, TitleFormatter.Format(input));
    }

    [Theory]
    [InlineData("Game__With___Multiple____Underscores.iso", "Game With Multiple Underscores")]
    [InlineData("  Game   With   Excessive   Spaces.iso  ", "Game With Excessive Spaces")]
    public void Format_CollapsesWhitespace(string input, string expected)
    {
        Assert.Equal(expected, TitleFormatter.Format(input));
    }

    [Fact]
    public void Format_WithPath_ExtractsFileNameAndCleans()
    {
        var path = Path.Combine("home", "user", "games", "Silent_Hill_2.iso");
        Assert.Equal("Silent Hill 2", TitleFormatter.Format(path));
    }
}
