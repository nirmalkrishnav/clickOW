using ClickOw.Settings;
using Xunit;

namespace ClickOw.Tests;

public class SizeScaleTests
{
    [Theory]
    [InlineData(SizePreset.Small, 30)]
    [InlineData(SizePreset.Medium, 44)]
    [InlineData(SizePreset.Large, 64)]
    public void ClickDiameter_ReturnsExpectedValue(SizePreset preset, double expected)
    {
        Assert.Equal(expected, SizeScale.ClickDiameter(preset));
    }

    [Theory]
    [InlineData(SizePreset.Small, 3)]
    [InlineData(SizePreset.Medium, 5)]
    [InlineData(SizePreset.Large, 8)]
    public void LaserThickness_ReturnsExpectedValue(SizePreset preset, double expected)
    {
        Assert.Equal(expected, SizeScale.LaserThickness(preset));
    }

    [Fact]
    public void ClickDiameter_IncreasesWithPreset()
    {
        Assert.True(SizeScale.ClickDiameter(SizePreset.Small) < SizeScale.ClickDiameter(SizePreset.Medium));
        Assert.True(SizeScale.ClickDiameter(SizePreset.Medium) < SizeScale.ClickDiameter(SizePreset.Large));
    }

    [Fact]
    public void LaserThickness_IncreasesWithPreset()
    {
        Assert.True(SizeScale.LaserThickness(SizePreset.Small) < SizeScale.LaserThickness(SizePreset.Medium));
        Assert.True(SizeScale.LaserThickness(SizePreset.Medium) < SizeScale.LaserThickness(SizePreset.Large));
    }
}
