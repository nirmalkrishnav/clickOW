using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ClickOw.Settings;

namespace ClickOw.Effects;

/// <summary>
/// Laser pointer mode: while dragging, appends freehand segments that fade out over
/// time, producing a trailing laser stroke.
/// </summary>
public sealed class LaserTrail
{
    private readonly Canvas _canvas;
    private readonly AppSettings _settings;
    private Point? _last;

    public LaserTrail(Canvas canvas, AppSettings settings)
    {
        _canvas = canvas;
        _settings = settings;
    }

    /// <summary>Begin a new stroke at the given canvas point.</summary>
    public void Begin(Point p)
    {
        _last = p;
        AddDot(p);
    }

    /// <summary>Extend the current stroke to a new canvas point.</summary>
    public void Extend(Point p)
    {
        if (_last is not Point start)
        {
            Begin(p);
            return;
        }

        AddSegment(start, p);
        _last = p;
    }

    /// <summary>End the current stroke.</summary>
    public void End()
    {
        _last = null;
    }

    private void AddSegment(Point a, Point b)
    {
        var color = ParseColor(_settings.LaserColorHex);
        var line = new Line
        {
            X1 = a.X,
            Y1 = a.Y,
            X2 = b.X,
            Y2 = b.Y,
            Stroke = new SolidColorBrush(color),
            StrokeThickness = _settings.LaserThickness,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeEndLineCap = PenLineCap.Round,
            IsHitTestVisible = false,
        };
        _canvas.Children.Add(line);
        FadeAndRemove(line);
    }

    private void AddDot(Point p)
    {
        double size = _settings.LaserThickness;
        var color = ParseColor(_settings.LaserColorHex);
        var dot = new Ellipse
        {
            Width = size,
            Height = size,
            Fill = new SolidColorBrush(color),
            IsHitTestVisible = false,
        };
        Canvas.SetLeft(dot, p.X - size / 2);
        Canvas.SetTop(dot, p.Y - size / 2);
        _canvas.Children.Add(dot);
        FadeAndRemove(dot);
    }

    private void FadeAndRemove(UIElement element)
    {
        var fade = new DoubleAnimation(1.0, 0.0, TimeSpan.FromMilliseconds(_settings.LaserFadeDuration))
        {
            FillBehavior = FillBehavior.Stop,
        };
        fade.Completed += (_, _) => _canvas.Children.Remove(element);
        element.BeginAnimation(UIElement.OpacityProperty, fade);
    }

    private static Color ParseColor(string argbHex)
    {
        try
        {
            return (Color)ColorConverter.ConvertFromString(argbHex);
        }
        catch
        {
            return Colors.Red;
        }
    }
}
