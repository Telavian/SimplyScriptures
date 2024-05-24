using System.Globalization;

namespace SimplyScriptures.Converters;

public class BooleanToButtonImageConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value == null || value is bool b == false
            ? "checkmark.png"
            : (object)(b
            ? "checkmark.png"
            : "cancel.png");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
