using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimplyScriptures.Common.Models;

namespace SimplyScriptures.Converters;

public class TopicItemsToInvertedVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || value is ContentTopicItem[] items == false)
        {
            return false;
        }

        return items.Length <= 0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
