using System.Windows;
using ClickOw.Effects;
using ClickOw.Hooks;
using ClickOw.Overlay;
using ClickOw.Settings;

namespace ClickOw;

/// <summary>
/// Central controller: owns the overlay, hooks and effect renderers, and translates
/// global mouse/keyboard events into click and laser visuals.
/// </summary>
public sealed class AppCoordinator : IDisposable
{
    private readonly AppSettings _settings;
    private readonly OverlayWindow _overlay;
    private readonly EffectRenderer _renderer;
    private readonly LaserTrail _laser;
    private readonly GlobalMouseHook _mouseHook;
    private readonly GlobalKeyboardHook _keyboardHook;

    private bool _leftDown;
    private bool _dragging;
    private Point _pressCanvasPoint;
    private int _pressScreenX;
    private int _pressScreenY;

    public event EventHandler? StateChanged;

    public AppCoordinator(AppSettings settings)
    {
        _settings = settings;

        _overlay = new OverlayWindow();
        _overlay.Show();

        _renderer = new EffectRenderer(_overlay.Surface, settings);
        _laser = new LaserTrail(_overlay.Surface, settings);

        _mouseHook = new GlobalMouseHook();
        _mouseHook.ButtonDown += OnButtonDown;
        _mouseHook.ButtonUp += OnButtonUp;
        _mouseHook.MouseMove += OnMouseMove;

        _keyboardHook = new GlobalKeyboardHook();
        _keyboardHook.ToggleEnabled += (_, _) => ToggleEnabled();
        _keyboardHook.ToggleLaser += (_, _) => ToggleLaser();

        _mouseHook.Start();
        _keyboardHook.Start();
    }

    public bool Enabled => _settings.Enabled;
    public bool LaserMode => _settings.LaserMode;

    public void ToggleEnabled()
    {
        _settings.Enabled = !_settings.Enabled;
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void ToggleLaser()
    {
        _settings.LaserMode = !_settings.LaserMode;
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnButtonDown(object? sender, MouseHookEventArgs e)
    {
        // Laser mode works independently of the click-highlight (Enabled) flag.
        if (!_settings.Enabled && !_settings.LaserMode)
        {
            return;
        }

        var canvasPoint = _overlay.ScreenToCanvas(e.X, e.Y);

        switch (e.Button)
        {
            case MouseButtonKind.Left:
                _leftDown = true;
                _dragging = false;
                _pressScreenX = e.X;
                _pressScreenY = e.Y;
                _pressCanvasPoint = canvasPoint;
                if (_settings.Enabled)
                {
                    _renderer.ShowPress(canvasPoint);
                }
                if (_settings.LaserMode)
                {
                    _laser.Begin(canvasPoint);
                }
                break;

            case MouseButtonKind.Right:
                if (_settings.Enabled)
                {
                    _renderer.ShowRightClick(canvasPoint);
                }
                break;
        }
    }

    private void OnButtonUp(object? sender, MouseHookEventArgs e)
    {
        if (e.Button != MouseButtonKind.Left)
        {
            return;
        }

        _leftDown = false;
        _dragging = false;

        if (!_settings.Enabled && !_settings.LaserMode)
        {
            return;
        }

        var canvasPoint = _overlay.ScreenToCanvas(e.X, e.Y);
        if (_settings.Enabled)
        {
            _renderer.ShowRelease(canvasPoint);
        }

        if (_settings.LaserMode)
        {
            _laser.End();
        }
    }

    private void OnMouseMove(object? sender, MouseMoveEventArgs e)
    {
        if ((!_settings.Enabled && !_settings.LaserMode) || !_leftDown)
        {
            return;
        }

        if (!_dragging)
        {
            double dx = e.X - _pressScreenX;
            double dy = e.Y - _pressScreenY;
            if (Math.Sqrt(dx * dx + dy * dy) < _settings.DragThreshold)
            {
                return;
            }

            _dragging = true;
        }

        var canvasPoint = _overlay.ScreenToCanvas(e.X, e.Y);
        if (_settings.LaserMode)
        {
            _laser.Extend(canvasPoint);
        }
        else if (_settings.Enabled && _settings.DragAnimation)
        {
            _renderer.ShowDragTrail(canvasPoint);
        }
    }

    public void Dispose()
    {
        _mouseHook.Dispose();
        _keyboardHook.Dispose();
        _overlay.Close();
    }
}
