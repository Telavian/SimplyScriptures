using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplyScriptures.Converters;

public class EnumToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null || value is Enum == false || parameter == null || parameter is Enum == false || value.GetType() != parameter.GetType())
        {
            return Visibility.Collapsed;
        }

        if (value?.ToString() == parameter?.ToString())
        {
            return Visibility.Visible;
        }

        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
