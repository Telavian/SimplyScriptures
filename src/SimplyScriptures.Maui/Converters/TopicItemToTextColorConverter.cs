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

public class TopicItemToTextColorConverter : IValueConverter
{
    public static object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is null || value is ContentTopicItem item == false
            ? value
            : item.Book switch
            {
                ScriptureBook.DC => Colors.Black,
                _ => Colors.White,
            };
    }

    public static object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
