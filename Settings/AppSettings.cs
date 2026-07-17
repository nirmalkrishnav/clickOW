using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace ClickOw.Settings;

/// <summary>
/// User-configurable settings. Colors are chosen from prebuilt <see cref="ColorTheme"/>
/// options and sizes from <see cref="SizePreset"/> options, keeping the model free of
/// WPF dependencies and serializing cleanly to JSON.
/// </summary>
public sealed class AppSettings : INotifyPropertyChanged
{
    // Base colors used when a color theme is set to Default.
    private const string ClickDefaultHex = "#FF3DA5FF"; // blue
    private const string DragDefaultHex = "#FFB68CFF";  // purple
    private const string LaserDefaultHex = "#FFFF4D4D"; // red

    private bool _enabled = true;
    private bool _laserMode;
    private bool _dragAnimation = true;
    private bool _runAtStartup = true;

    private ColorTheme _clickColor = ColorTheme.Default;
    private ColorTheme _dragColor = ColorTheme.Default;
    private ColorTheme _laserColor = ColorTheme.Default;

    private SizePreset _clickSizePreset = SizePreset.Medium;
    private SizePreset _laserThicknessPreset = SizePreset.Medium;

    public bool Enabled
    {
        get => _enabled;
        set => Set(ref _enabled, value);
    }

    public bool LaserMode
    {
        get => _laserMode;
        set => Set(ref _laserMode, value);
    }

    /// <summary>When enabled, dragging leaves a trailing dot effect.</summary>
    public bool DragAnimation
    {
        get => _dragAnimation;
        set => Set(ref _dragAnimation, value);
    }

    public bool RunAtStartup
    {
        get => _runAtStartup;
        set => Set(ref _runAtStartup, value);
    }

    public ColorTheme ClickColor
    {
        get => _clickColor;
        set => Set(ref _clickColor, value, nameof(ClickColor), nameof(ClickColorHex));
    }

    public ColorTheme DragColor
    {
        get => _dragColor;
        set => Set(ref _dragColor, value, nameof(DragColor), nameof(DragColorHex));
    }

    public ColorTheme LaserColor
    {
        get => _laserColor;
        set => Set(ref _laserColor, value, nameof(LaserColor), nameof(LaserColorHex));
    }

    public SizePreset ClickSizePreset
    {
        get => _clickSizePreset;
        set => Set(ref _clickSizePreset, value, nameof(ClickSizePreset), nameof(ClickSize));
    }

    public SizePreset LaserThicknessPreset
    {
        get => _laserThicknessPreset;
        set => Set(ref _laserThicknessPreset, value, nameof(LaserThicknessPreset), nameof(LaserThickness));
    }

    // --- Resolved values consumed by the renderers (not persisted directly) ---

    [JsonIgnore]
    public string ClickColorHex => ColorPalette.ResolveHex(_clickColor, ClickDefaultHex);

    [JsonIgnore]
    public string DragColorHex => ColorPalette.ResolveHex(_dragColor, DragDefaultHex);

    [JsonIgnore]
    public string LaserColorHex => ColorPalette.ResolveHex(_laserColor, LaserDefaultHex);

    [JsonIgnore]
    public double ClickSize => SizeScale.ClickDiameter(_clickSizePreset);

    [JsonIgnore]
    public double LaserThickness => SizeScale.LaserThickness(_laserThicknessPreset);

    // --- Fixed (non-configurable) timing and threshold values ---

    [JsonIgnore]
    public double EffectDuration => 550;    // ms

    [JsonIgnore]
    public double DragThreshold => 6;       // px before movement counts as a drag

    [JsonIgnore]
    public double LaserFadeDuration => 700; // ms for a laser segment to fade

    public event PropertyChangedEventHandler? PropertyChanged;

    private void Set<T>(ref T field, T value, [CallerMemberName] string? name = null, string? alsoNotify = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        if (alsoNotify is not null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(alsoNotify));
        }
    }
}
