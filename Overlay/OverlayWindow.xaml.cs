using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using ClickOW.Interop;

namespace ClickOW.Overlay;

/// <summary>
/// A borderless, transparent, click-through window that spans the entire virtual
/// desktop and hosts the click/laser effects on its <see cref="Canvas"/>.
/// </summary>
public partial class OverlayWindow : Window
{
    private readonly DispatcherTimer _topmostTimer;

    public OverlayWindow()
    {
        InitializeComponent();

        // WPF's device-independent units are anchored at the primary screen origin (0,0).
        // Position/size to the full virtual screen so effects render on every monitor.
        Left = SystemParameters.VirtualScreenLeft;
        Top = SystemParameters.VirtualScreenTop;
        Width = SystemParameters.VirtualScreenWidth;
        Height = SystemParameters.VirtualScreenHeight;

        // Other topmost windows (or fullscreen apps) can steal z-order; re-assert periodically.
        _topmostTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
        _topmostTimer.Tick += (_, _) => ReassertTopmost();
    }

    public Canvas Surface => RootCanvas;

    /// <summary>The window's virtual-screen origin in DIP, used to convert screen points.</summary>
    public Point VirtualOrigin => new(SystemParameters.VirtualScreenLeft, SystemParameters.VirtualScreenTop);

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        var helper = new WindowInteropHelper(this);
        int exStyle = NativeMethods.GetWindowLong(helper.Handle, NativeMethods.GWL_EXSTYLE);
        exStyle |= NativeMethods.WS_EX_LAYERED
                 | NativeMethods.WS_EX_TRANSPARENT   // click-through
                 | NativeMethods.WS_EX_NOACTIVATE    // never takes focus
                 | NativeMethods.WS_EX_TOOLWINDOW;   // hidden from Alt+Tab
        NativeMethods.SetWindowLong(helper.Handle, NativeMethods.GWL_EXSTYLE, exStyle);

        _topmostTimer.Start();
    }

    /// <summary>
    /// Converts a physical screen point (from the mouse hook) into a coordinate on
    /// the overlay canvas, accounting for the current DPI.
    /// </summary>
    public Point ScreenToCanvas(int screenX, int screenY)
    {
        var source = PresentationSource.FromVisual(this);
        double dpiX = 1.0, dpiY = 1.0;
        if (source?.CompositionTarget is CompositionTarget ct)
        {
            dpiX = ct.TransformFromDevice.M11;
            dpiY = ct.TransformFromDevice.M22;
        }

        double dipX = screenX * dpiX;
        double dipY = screenY * dpiY;
        return new Point(dipX - VirtualOrigin.X, dipY - VirtualOrigin.Y);
    }

    private void ReassertTopmost()
    {
        if (Topmost)
        {
            Topmost = false;
            Topmost = true;
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        _topmostTimer.Stop();
        base.OnClosed(e);
    }
}
