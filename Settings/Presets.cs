namespace ClickOW.Settings;

/// <summary>Standard size options offered in the UI.</summary>
public enum SizePreset
{
    Small,
    Medium,
    Large,
}

/// <summary>
/// Prebuilt color themes. <see cref="Default"/> resolves to a sensible per-context
/// color; the rest are fixed "exotic" palettes shared by clicks, laser and drag.
/// </summary>
public enum ColorTheme
{
    Default,
    TealNebula,
    CoralBlush,
    AmethystHaze,
    GoldenAurora,
    MintMirage,
}

/// <summary>Maps color themes to ARGB hex strings, keeping the settings model WPF-free.</summary>
public static class ColorPalette
{
    public static readonly ColorTheme[] All =
    {
        ColorTheme.Default,
        ColorTheme.TealNebula,
        ColorTheme.CoralBlush,
        ColorTheme.AmethystHaze,
        ColorTheme.GoldenAurora,
        ColorTheme.MintMirage,
    };

    /// <summary>
    /// Resolves a theme to an ARGB hex string. <paramref name="fallbackHex"/> is used
    /// for <see cref="ColorTheme.Default"/> so each context keeps its own base color.
    /// </summary>
    public static string ResolveHex(ColorTheme theme, string fallbackHex) => theme switch
    {
        ColorTheme.TealNebula => "#FF14B8A6",
        ColorTheme.CoralBlush => "#FFFF6B81",
        ColorTheme.AmethystHaze => "#FFB57EDC",
        ColorTheme.GoldenAurora => "#FFFFC24B",
        ColorTheme.MintMirage => "#FF6EE7B7",
        _ => fallbackHex,
    };
}

/// <summary>Maps size presets to concrete DIP values.</summary>
public static class SizeScale
{
    public static double ClickDiameter(SizePreset preset) => preset switch
    {
        SizePreset.Small => 30,
        SizePreset.Large => 64,
        _ => 44,
    };

    public static double LaserThickness(SizePreset preset) => preset switch
    {
        SizePreset.Small => 3,
        SizePreset.Large => 8,
        _ => 5,
    };
}
