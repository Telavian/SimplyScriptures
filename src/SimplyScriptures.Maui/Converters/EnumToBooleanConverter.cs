using System.Globalization;

namespace SimplyScriptures.Converters;

public class BooleanToInvertedBooleanConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null || value is bool item == false)
        {
            return false;
        }

        return !item;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
