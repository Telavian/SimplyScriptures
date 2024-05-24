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

public class TopicItemToColorConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is null || value is ContentTopicItem item == false
            ? value
            : item.Book.IsOldTestament()
            ? Color.FromRgb(89, 74, 226)
            : item.Book switch
            {
                ScriptureBook.NT => Color.FromRgb(33, 150, 243),
                ScriptureBook.BM => Color.FromRgb(0, 200, 83),
                ScriptureBook.DC => Colors.DarkGray,
                _ => Colors.White,
            };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
