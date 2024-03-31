using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplyScriptures.Converters;

public class EnumToVisibilityConverter : IValueConverter
{
    public static object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value == null || (value is Enum) == false || parameter == null || (parameter is Enum) == false || value.GetType() != parameter.GetType()
            ? Visibility.Collapsed
            : (object)(value?.ToString() == parameter?.ToString() ? Visibility.Visible : Visibility.Collapsed);
    }

    public static object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
