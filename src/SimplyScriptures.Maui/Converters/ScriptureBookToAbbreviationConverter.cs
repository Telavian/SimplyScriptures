using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimplyScriptures.Common.Enums;
using SimplyScriptures.Common.Extensions;

namespace SimplyScriptures.Converters;

public class ScriptureBookToAbbreviationConverter : IValueConverter
{
    public static object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        return value == null || value is ScriptureBook book == false ? "" : (object)book.ToAbbreviatedDisplayString();
    }

    public static object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
