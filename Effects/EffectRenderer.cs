using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ClickOw.Settings;

namespace ClickOw.Effects;

/// <summary>
/// Renders the distinct click visuals (press, release, right-click, drag) onto the
/// overlay canvas. Each effect is a short WPF animation that removes itself when done.
/// </summary>
public sealed class EffectRenderer
{
    private readonly Canvas _canvas;
    private readonly AppSettings _settings;

    public EffectRenderer(Canvas canvas, AppSettings settings)
    {
        _canvas = canvas;
        _settings = settings;
    }

    /// <summary>Left button pressed: a filled circle that expands and fades out.</summary>
    public void ShowPress(Point p)
    {
        double size = _settings.ClickSize;
        var color = ParseColor(_settings.ClickColorHex);

        var ellipse = new Ellipse
        {
            Width = size,
            Height = size,
            Fill = new SolidColorBrush(color),
            IsHitTestVisible = false,
            RenderTransformOrigin = new Point(0.5, 0.5),
        };
        AddCentered(ellipse, p, size);
        AnimateRipple(ellipse, fromScale: 0.3, toScale: 1.0, fromOpacity: 0.55, toOpacity: 0.0);
    }

    /// <summary>Left button released: a thin ring that snaps outward and fades quickly.</summary>
    public void ShowRelease(Point p)
    {
        double size = _settings.ClickSize;
        var color = ParseColor(_settings.ClickColorHex);

        var ring = new Ellipse
        {
            Width = size,
            Height = size,
            Stroke = new SolidColorBrush(color),
            StrokeThickness = 3,
            Fill = Brushes.Transparent,
            IsHitTestVisible = false,
            RenderTransformOrigin = new Point(0.5, 0.5),
        };
        AddCentered(ring, p, size);
        AnimateRipple(ring, fromScale: 0.6, toScale: 1.25, fromOpacity: 0.9, toOpacity: 0.0, durationScale: 0.65);
    }

    /// <summary>Right button pressed: a double ring in the right-click color.</summary>
    public void ShowRightClick(Point p)
    {
        double size = _settings.ClickSize * 1.1;
        var color = ParseColor(_settings.ClickColorHex);

        var ring = new Ellipse
        {
            Width = size,
            Height = size,
            Stroke = new SolidColorBrush(color),
            StrokeThickness = 4,
            StrokeDashArray = new DoubleCollection { 3, 2 },
            Fill = Brushes.Transparent,
            IsHitTestVisible = false,
            RenderTransformOrigin = new Point(0.5, 0.5),
        };
        AddCentered(ring, p, size);
        AnimateRipple(ring, fromScale: 0.35, toScale: 1.1, fromOpacity: 0.85, toOpacity: 0.0);
    }

    /// <summary>Drag: a small trailing dot that follows the cursor and fades out.</summary>
    public void ShowDragTrail(Point p)
    {
        double size = _settings.ClickSize * 0.4;
        var color = ParseColor(_settings.DragColorHex);

        var dot = new Ellipse
        {
            Width = size,
            Height = size,
            Fill = new SolidColorBrush(color),
            IsHitTestVisible = false,
            RenderTransformOrigin = new Point(0.5, 0.5),
        };
        AddCentered(dot, p, size);

        var fade = new DoubleAnimation(0.5, 0.0, TimeSpan.FromMilliseconds(_settings.EffectDuration * 0.8))
        {
            FillBehavior = FillBehavior.Stop,
        };
        fade.Completed += (_, _) => _canvas.Children.Remove(dot);
        dot.BeginAnimation(UIElement.OpacityProperty, fade);
    }

    private void AddCentered(FrameworkElement element, Point center, double size)
    {
        Canvas.SetLeft(element, center.X - size / 2);
        Canvas.SetTop(element, center.Y - size / 2);
        _canvas.Children.Add(element);
    }

    private void AnimateRipple(
        Shape shape,
        double fromScale,
        double toScale,
        double fromOpacity,
        double toOpacity,
        double durationScale = 1.0)
    {
        var duration = TimeSpan.FromMilliseconds(_settings.EffectDuration * durationScale);

        var scale = new ScaleTransform(fromScale, fromScale);
        shape.RenderTransform = scale;

        var ease = new CubicEase { EasingMode = EasingMode.EaseOut };
        var scaleAnim = new DoubleAnimation(fromScale, toScale, duration) { EasingFunction = ease };
        var opacityAnim = new DoubleAnimation(fromOpacity, toOpacity, duration)
        {
            EasingFunction = ease,
            FillBehavior = FillBehavior.Stop,
        };
        opacityAnim.Completed += (_, _) => _canvas.Children.Remove(shape);

        scale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
        scale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);
        shape.BeginAnimation(UIElement.OpacityProperty, opacityAnim);
    }

    private static Color ParseColor(string argbHex)
    {
        try
        {
            return (Color)ColorConverter.ConvertFromString(argbHex);
        }
        catch
        {
            return Colors.DeepSkyBlue;
        }
    }
}
