using System.Globalization;

namespace SimplyScriptures.Converters;

public class BooleanToButtonImageConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null || value is bool b == false)
        {
            return "checkmark.png";
        }

        return b
            ? "checkmark.png"
            : "cancel.png";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
