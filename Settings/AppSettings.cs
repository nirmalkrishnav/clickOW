using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ClickOw.Settings;

/// <summary>
/// User-configurable settings. Values are stored as ARGB hex strings so the model
/// stays free of WPF dependencies and serializes cleanly to JSON.
/// </summary>
public sealed class AppSettings : INotifyPropertyChanged
{
    private bool _enabled = true;
    private bool _laserMode;
    private bool _runAtStartup = true;

    private string _pressColor = "#FF3DA5FF";   // blue
    private string _releaseColor = "#FF7FE0A0"; // green
    private string _rightColor = "#FFFF9F45";   // orange
    private string _dragColor = "#FFB68CFF";    // purple
    private string _laserColor = "#FFFF4D4D";   // red

    private double _clickSize = 44;      // diameter in DIP of a ripple at full size
    private double _effectDuration = 550; // ms
    private double _dragThreshold = 6;   // px before movement counts as a drag
    private double _laserThickness = 5;  // stroke thickness in DIP
    private double _laserFadeDuration = 700; // ms for a laser segment to fade

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

    public bool RunAtStartup
    {
        get => _runAtStartup;
        set => Set(ref _runAtStartup, value);
    }

    public string PressColor
    {
        get => _pressColor;
        set => Set(ref _pressColor, value);
    }

    public string ReleaseColor
    {
        get => _releaseColor;
        set => Set(ref _releaseColor, value);
    }

    public string RightColor
    {
        get => _rightColor;
        set => Set(ref _rightColor, value);
    }

    public string DragColor
    {
        get => _dragColor;
        set => Set(ref _dragColor, value);
    }

    public string LaserColor
    {
        get => _laserColor;
        set => Set(ref _laserColor, value);
    }

    public double ClickSize
    {
        get => _clickSize;
        set => Set(ref _clickSize, value);
    }

    public double EffectDuration
    {
        get => _effectDuration;
        set => Set(ref _effectDuration, value);
    }

    public double DragThreshold
    {
        get => _dragThreshold;
        set => Set(ref _dragThreshold, value);
    }

    public double LaserThickness
    {
        get => _laserThickness;
        set => Set(ref _laserThickness, value);
    }

    public double LaserFadeDuration
    {
        get => _laserFadeDuration;
        set => Set(ref _laserFadeDuration, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
