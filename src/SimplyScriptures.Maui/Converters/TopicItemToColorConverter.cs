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
        if (value is null || value is ContentTopicItem item == false)
        {
            return value;
        }

        if (item.Book.IsOldTestament())
        {
            return Color.FromRgb(89, 74, 226);
        }

        switch (item.Book)
        {
            case ScriptureBook.NT:
                return Color.FromRgb(33, 150, 243);
            case ScriptureBook.BM:
                return Color.FromRgb(0, 200, 83);
            case ScriptureBook.DC:
                return Colors.DarkGray;
            default:
                return Colors.White;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
