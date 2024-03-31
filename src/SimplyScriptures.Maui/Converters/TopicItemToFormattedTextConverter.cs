using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimplyScriptures.Common.Html.Models;
using SimplyScriptures.Common.Models;
using SimplyScriptures.Models;
using SimplyScriptures.Services;

namespace SimplyScriptures.Converters;

public class TopicItemToFormattedTextConverter : IValueConverter
{
    public static object? Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is null || value is ContentTopicItem item == false)
        {
            return Array.Empty<FormattedTextItem>();
        }

        item.BuildFormattedItems();
        return item.FormattedItems;
    }

    public static object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
