using System.Globalization;

namespace SimplyScriptures.Converters;

public class BooleanToInvertedBooleanConverter : IValueConverter
{
    public static object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value == null || value is bool item == false ? false : (object)!item;
    }

    public static object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
