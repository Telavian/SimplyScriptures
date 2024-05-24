using System.Globalization;
using SimplyScriptures.Common.Models;

namespace SimplyScriptures.Converters;

public class TopicItemToDividerVisibilityConverter : IMultiValueConverter
{
    public object Convert(object[]? values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values == null || values.Length != 2 ||
            values[0] is ContentTopicItem topicItem == false ||
            values[1] is ContentTopicItem[] topicItems == false)
        {
            return true;
        }

        return topicItem != topicItems.LastOrDefault();
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        return new[] { Binding.DoNothing };
    }
}
