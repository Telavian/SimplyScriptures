using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplyScriptures.Converters;

public class SelectedMenuContentItemToColorConverter : IValueConverter
{
    public static object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch
        {
            bool b when b => Colors.Green,
            _ => App.IsDarkTheme
                                ? Colors.White
                                : Colors.Black,
        };
    }

    public static object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
