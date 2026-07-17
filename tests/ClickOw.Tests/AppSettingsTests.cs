using System.ComponentModel;
using ClickOW.Settings;
using Xunit;

namespace ClickOw.Tests;

public class AppSettingsTests
{
    [Fact]
    public void Defaults_AreExpected()
    {
        var settings = new AppSettings();

        Assert.True(settings.Enabled);
        Assert.False(settings.LaserMode);
        Assert.False(settings.DragAnimation);
        Assert.True(settings.RunAtStartup);
        Assert.Equal(ColorTheme.Default, settings.ClickColor);
        Assert.Equal(ColorTheme.Default, settings.DragColor);
        Assert.Equal(ColorTheme.Default, settings.LaserColor);
        Assert.Equal(SizePreset.Medium, settings.ClickSizePreset);
        Assert.Equal(SizePreset.Medium, settings.LaserThicknessPreset);
    }

    [Fact]
    public void SettingProperty_RaisesPropertyChanged()
    {
        var settings = new AppSettings();
        var changed = new List<string?>();
        settings.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        settings.Enabled = false;

        Assert.Contains(nameof(AppSettings.Enabled), changed);
    }

    [Fact]
    public void SettingSameValue_DoesNotRaisePropertyChanged()
    {
        var settings = new AppSettings();
        var raised = false;
        settings.PropertyChanged += (_, _) => raised = true;

        settings.Enabled = settings.Enabled;

        Assert.False(raised);
    }

    [Fact]
    public void SettingClickColor_AlsoNotifiesResolvedHex()
    {
        var settings = new AppSettings();
        var changed = new List<string?>();
        settings.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        settings.ClickColor = ColorTheme.TealNebula;

        Assert.Contains(nameof(AppSettings.ClickColor), changed);
        Assert.Contains(nameof(AppSettings.ClickColorHex), changed);
    }

    [Fact]
    public void SettingClickSizePreset_AlsoNotifiesClickSize()
    {
        var settings = new AppSettings();
        var changed = new List<string?>();
        settings.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        settings.ClickSizePreset = SizePreset.Large;

        Assert.Contains(nameof(AppSettings.ClickSizePreset), changed);
        Assert.Contains(nameof(AppSettings.ClickSize), changed);
    }

    [Fact]
    public void ClickColorHex_ReflectsSelectedTheme()
    {
        var settings = new AppSettings { ClickColor = ColorTheme.GoldenAurora };
        Assert.Equal(ColorPalette.ResolveHex(ColorTheme.GoldenAurora, settings.ClickColorHex), settings.ClickColorHex);
        Assert.Equal("#FFFFC24B", settings.ClickColorHex);
    }

    [Fact]
    public void ClickSize_ReflectsSelectedPreset()
    {
        var settings = new AppSettings { ClickSizePreset = SizePreset.Large };
        Assert.Equal(SizeScale.ClickDiameter(SizePreset.Large), settings.ClickSize);
    }

    [Fact]
    public void LaserThickness_ReflectsSelectedPreset()
    {
        var settings = new AppSettings { LaserThicknessPreset = SizePreset.Small };
        Assert.Equal(SizeScale.LaserThickness(SizePreset.Small), settings.LaserThickness);
    }

    [Fact]
    public void RightClickColorHex_IsFixed()
    {
        var settings = new AppSettings();
        Assert.Equal("#FFFF7043", settings.RightClickColorHex);
    }

    [Fact]
    public void FixedTimings_HaveExpectedValues()
    {
        var settings = new AppSettings();
        Assert.Equal(550, settings.EffectDuration);
        Assert.Equal(6, settings.DragThreshold);
        Assert.Equal(700, settings.LaserFadeDuration);
    }
}
