using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplyScriptures.Converters;

public class SelectedMenuContentItemToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        switch (value)
        {
            case bool b when b:
                return Colors.Green;
            default:
                return App.IsDarkTheme
                    ? Colors.White
                    : Colors.Black;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
