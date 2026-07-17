using ClickOW.Settings;
using Xunit;

namespace ClickOw.Tests;

public class ColorPaletteTests
{
    [Fact]
    public void All_ContainsEveryTheme()
    {
        var expected = Enum.GetValues<ColorTheme>();
        Assert.Equal(expected.Length, ColorPalette.All.Length);
        Assert.Equal(expected.OrderBy(x => x), ColorPalette.All.OrderBy(x => x));
    }

    [Fact]
    public void All_StartsWithDefault()
    {
        Assert.Equal(ColorTheme.Default, ColorPalette.All[0]);
    }

    [Fact]
    public void ResolveHex_Default_ReturnsFallback()
    {
        const string fallback = "#FF123456";
        Assert.Equal(fallback, ColorPalette.ResolveHex(ColorTheme.Default, fallback));
    }

    [Theory]
    [InlineData(ColorTheme.TealNebula, "#FF14B8A6")]
    [InlineData(ColorTheme.CoralBlush, "#FFFF6B81")]
    [InlineData(ColorTheme.AmethystHaze, "#FFB57EDC")]
    [InlineData(ColorTheme.GoldenAurora, "#FFFFC24B")]
    [InlineData(ColorTheme.MintMirage, "#FF6EE7B7")]
    public void ResolveHex_NonDefault_IgnoresFallback(ColorTheme theme, string expected)
    {
        Assert.Equal(expected, ColorPalette.ResolveHex(theme, "#FFFFFFFF"));
    }

    [Fact]
    public void ResolveHex_EveryNonDefaultTheme_ReturnsValidArgbHex()
    {
        foreach (ColorTheme theme in ColorPalette.All.Where(t => t != ColorTheme.Default))
        {
            string hex = ColorPalette.ResolveHex(theme, "#FF000000");
            Assert.Matches("^#[0-9A-Fa-f]{8}$", hex);
        }
    }
}
