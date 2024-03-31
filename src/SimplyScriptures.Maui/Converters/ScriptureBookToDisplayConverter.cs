﻿using System.Globalization;
using SimplyScriptures.Common.Enums;
using SimplyScriptures.Common.Extensions;

namespace SimplyScriptures.Converters;

public class ScriptureBookToDisplayConverter : IValueConverter
{
    public static object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value == null || value is ScriptureBook book == false ? "" : (object)book.ToDisplayString();
    }

    public static object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
