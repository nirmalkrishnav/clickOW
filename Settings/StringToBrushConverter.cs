using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ClickOw.Settings;

/// <summary>Converts an ARGB hex string into a <see cref="SolidColorBrush"/> for swatch previews.</summary>
public sealed class StringToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        try
        {
            if (value is string s && !string.IsNullOrWhiteSpace(s))
            {
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString(s));
            }
        }
        catch
        {
            // fall through to transparent
        }

        return Brushes.Transparent;
    }

    public object Value(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
