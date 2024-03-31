using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimplyScriptures.Common.Models;

namespace SimplyScriptures.Converters;

public class ArrayItemsToInvertedVisibilityConverter : IValueConverter
{
    public static object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        return value == null || value is Array items == false ? false : (object)(items.Length <= 0);
    }

    public static object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
