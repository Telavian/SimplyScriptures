using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimplyScriptures.Common.Enums;
using SimplyScriptures.Common.Extensions;
using SimplyScriptures.Common.Models;

namespace SimplyScriptures.Converters;

public class ScriptureBookToTextColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is null || value is ScriptureBook book == false)
        {
            return value;
        }

        return book.IsDoctrineAndCovenants()
            ? Colors.Black 
            : Colors.White;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
