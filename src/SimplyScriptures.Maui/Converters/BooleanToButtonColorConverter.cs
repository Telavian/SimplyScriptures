using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplyScriptures.Converters;

public class BooleanToButtonColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null || value is bool b == false)
        {
            return Color.FromRgb(0, 200, 83); // Green
        }

        return b
            ? Color.FromRgb(0, 200, 83) // Green
            : Color.FromRgb(244, 67, 54); // Red
    }

    public static object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
