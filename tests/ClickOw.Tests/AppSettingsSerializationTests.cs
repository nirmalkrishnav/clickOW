using System.Text.Json;
using ClickOW.Settings;
using Xunit;

namespace ClickOw.Tests;

public class AppSettingsSerializationTests
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    [Fact]
    public void RoundTrip_PreservesPersistedValues()
    {
        var original = new AppSettings
        {
            Enabled = false,
            LaserMode = true,
            DragAnimation = true,
            RunAtStartup = false,
            ClickColor = ColorTheme.TealNebula,
            DragColor = ColorTheme.CoralBlush,
            LaserColor = ColorTheme.MintMirage,
            ClickSizePreset = SizePreset.Large,
            LaserThicknessPreset = SizePreset.Small,
        };

        string json = JsonSerializer.Serialize(original, Options);
        var restored = JsonSerializer.Deserialize<AppSettings>(json, Options);

        Assert.NotNull(restored);
        Assert.Equal(original.Enabled, restored!.Enabled);
        Assert.Equal(original.LaserMode, restored.LaserMode);
        Assert.Equal(original.DragAnimation, restored.DragAnimation);
        Assert.Equal(original.RunAtStartup, restored.RunAtStartup);
        Assert.Equal(original.ClickColor, restored.ClickColor);
        Assert.Equal(original.DragColor, restored.DragColor);
        Assert.Equal(original.LaserColor, restored.LaserColor);
        Assert.Equal(original.ClickSizePreset, restored.ClickSizePreset);
        Assert.Equal(original.LaserThicknessPreset, restored.LaserThicknessPreset);
    }

    [Fact]
    public void Serialize_DoesNotIncludeResolvedProperties()
    {
        var settings = new AppSettings();

        string json = JsonSerializer.Serialize(settings, Options);

        Assert.DoesNotContain("\"ClickColorHex\"", json);
        Assert.DoesNotContain("\"ClickSize\"", json);
        Assert.DoesNotContain("\"LaserThickness\"", json);
        Assert.DoesNotContain("\"RightClickColorHex\"", json);
        Assert.DoesNotContain("\"EffectDuration\"", json);
    }

    [Fact]
    public void Deserialize_InvalidJson_Throws()
    {
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<AppSettings>("{ not valid", Options));
    }
}
