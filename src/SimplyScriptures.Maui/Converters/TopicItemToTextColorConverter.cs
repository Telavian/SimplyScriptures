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
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is null || value is ContentTopicItem item == false)
        {
            return value;
        }

        switch (item.Book)
        {
            case ScriptureBook.DC:
                return Colors.Black;
            default:
                return Colors.White;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
