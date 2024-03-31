using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplyScriptures.Converters;

public class ColorToVisibilityConverter : IValueConverter
{
    public static object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value == null || value is Color c == false ? false : c.Equals(Colors.Transparent) ? true : (object)false;
    }

    public static object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
