using System.Globalization;
using SimplyScriptures.Common.Models;

namespace SimplyScriptures.Converters;

public class TopicItemToDividerVisibilityConverter : IMultiValueConverter
{
    public static object Convert(object[]? values, Type targetType, object parameter, CultureInfo culture)
    {


/* Unmerged change from project 'SimplyScriptures.Maui(net8.0-ios)'
Before:
/* Unmerged change from project 'SimplyScriptures.Maui(net8.0-ios)'
Before:
        if (values == null || values.Length != 2 || 
            values[0] is ContentTopicItem topicItem == false ||
            values[1] is ContentTopicItem[] topicItems == false)
        {
            return true;
        }

        return topicItem != topicItems.LastOrDefault();
After:
        return values == null || values.Length != 2 ||
            values[0] is ContentTopicItem topicItem == false ||
            values[1] is ContentTopicItem[] topicItems == false
            ? true
            : (object)(topicItem != topicItems.LastOrDefault());
*/

/* Unmerged change from project 'SimplyScriptures.Maui(net8.0-maccatalyst)'
Before:
        if (values == null || values.Length != 2 || 
            values[0] is ContentTopicItem topicItem == false ||
            values[1] is ContentTopicItem[] topicItems == false)
        {
            return true;
        }

        return topicItem != topicItems.LastOrDefault();
After:
        return values == null || values.Length != 2 ||
            values[0] is ContentTopicItem topicItem == false ||
            values[1] is ContentTopicItem[] topicItems == false
            ? true
            : (object)(topicItem != topicItems.LastOrDefault());
*/

/* Unmerged change from project 'SimplyScriptures.Maui(net8.0-windows10.0.19041.0)'
Before:
        if (values == null || values.Length != 2 || 
            values[0] is ContentTopicItem topicItem == false ||
            values[1] is ContentTopicItem[] topicItems == false)
        {
            return true;
        }

        return topicItem != topicItems.LastOrDefault();
After:
        return values == null || values.Length != 2 ||
            values[0] is ContentTopicItem topicItem == false ||
            values[1] is ContentTopicItem[] topicItems == false
            ? true
            : (object)(topicItem != topicItems.LastOrDefault());
*/
        if (values == null || values.Length != 2 ||
            values[0] is ContentTopicItem topicItem == false ||
            values[1] is ContentTopicItem[] topicItems == false)
        {
            return true;
        }

        return topicItem != topicItems.LastOrDefault();
After:
        /* Unmerged change from project 'SimplyScriptures.Maui(net8.0-ios)'
        Before:
                if (values == null || values.Length != 2 || 
                    values[0] is ContentTopicItem topicItem == false ||
                    values[1] is ContentTopicItem[] topicItems == false)
                {
                    return true;
                }

                return topicItem != topicItems.LastOrDefault();
        After:
                return values == null || values.Length != 2 ||
                    values[0] is ContentTopicItem topicItem == false ||
                    values[1] is ContentTopicItem[] topicItems == false
                    ? true
                    : (object)(topicItem != topicItems.LastOrDefault());
        */

        /* Unmerged change from project 'SimplyScriptures.Maui(net8.0-maccatalyst)'
        Before:
                if (values == null || values.Length != 2 || 
                    values[0] is ContentTopicItem topicItem == false ||
                    values[1] is ContentTopicItem[] topicItems == false)
                {
                    return true;
                }

                return topicItem != topicItems.LastOrDefault();
        After:
                return values == null || values.Length != 2 ||
                    values[0] is ContentTopicItem topicItem == false ||
                    values[1] is ContentTopicItem[] topicItems == false
                    ? true
                    : (object)(topicItem != topicItems.LastOrDefault());
        */

        /* Unmerged change from project 'SimplyScriptures.Maui(net8.0-windows10.0.19041.0)'
        Before:
                if (values == null || values.Length != 2 || 
                    values[0] is ContentTopicItem topicItem == false ||
                    values[1] is ContentTopicItem[] topicItems == false)
                {
                    return true;
                }

                return topicItem != topicItems.LastOrDefault();
        After:
                return values == null || values.Length != 2 ||
                    values[0] is ContentTopicItem topicItem == false ||
                    values[1] is ContentTopicItem[] topicItems == false
                    ? true
                    : (object)(topicItem != topicItems.LastOrDefault());
        */
        return values == null || values.Length != 2 ||
            values[0] is ContentTopicItem topicItem == false ||
            values[1] is ContentTopicItem[] topicItems == false
            ? true
            : (object)(topicItem != topicItems.LastOrDefault());
*/

/* Unmerged change from project 'SimplyScriptures.Maui(net8.0-maccatalyst)'
Before:
/* Unmerged change from project 'SimplyScriptures.Maui(net8.0-ios)'
Before:
        if (values == null || values.Length != 2 || 
            values[0] is ContentTopicItem topicItem == false ||
            values[1] is ContentTopicItem[] topicItems == false)
        {
            return true;
        }

        return topicItem != topicItems.LastOrDefault();
After:
        return values == null || values.Length != 2 ||
            values[0] is ContentTopicItem topicItem == false ||
            values[1] is ContentTopicItem[] topicItems == false
            ? true
            : (object)(topicItem != topicItems.LastOrDefault());
*/

/* Unmerged change from project 'SimplyScriptures.Maui(net8.0-maccatalyst)'
Before:
        if (values == null || values.Length != 2 || 
            values[0] is ContentTopicItem topicItem == false ||
            values[1] is ContentTopicItem[] topicItems == false)
        {
            return true;
        }

        return topicItem != topicItems.LastOrDefault();
After:
        return values == null || values.Length != 2 ||
            values[0] is ContentTopicItem topicItem == false ||
            values[1] is ContentTopicItem[] topicItems == false
            ? true
            : (object)(topicItem != topicItems.LastOrDefault());
*/

/* Unmerged change from project 'SimplyScriptures.Maui(net8.0-windows10.0.19041.0)'
Before:
        if (values == null || values.Length != 2 || 
            values[0] is ContentTopicItem topicItem == false ||
            values[1] is ContentTopicItem[] topicItems == false)
        {
            return true;
        }

        return topicItem != topicItems.LastOrDefault();
After:
        return values == null || values.Length != 2 ||
            values[0] is ContentTopicItem topicItem == false ||
            values[1] is ContentTopicItem[] topicItems == false
            ? true
            : (object)(topicItem != topicItems.LastOrDefault());
*/
        if (values == null || values.Length != 2 ||
            values[0] is ContentTopicItem topicItem == false ||
            values[1] is ContentTopicItem[] topicItems == false)
        {
            return true;
        }

        return topicItem != topicItems.LastOrDefault();
After:
        /* Unmerged change from project 'SimplyScriptures.Maui(net8.0-ios)'
        Before:
                if (values == null || values.Length != 2 || 
                    values[0] is ContentTopicItem topicItem == false ||
                    values[1] is ContentTopicItem[] topicItems == false)
                {
                    return true;
                }

                return topicItem != topicItems.LastOrDefault();
        After:
                return values == null || values.Length != 2 ||
                    values[0] is ContentTopicItem topicItem == false ||
                    values[1] is ContentTopicItem[] topicItems == false
                    ? true
                    : (object)(topicItem != topicItems.LastOrDefault());
        */

        /* Unmerged change from project 'SimplyScriptures.Maui(net8.0-maccatalyst)'
        Before:
                if (values == null || values.Length != 2 || 
                    values[0] is ContentTopicItem topicItem == false ||
                    values[1] is ContentTopicItem[] topicItems == false)
                {
                    return true;
                }

                return topicItem != topicItems.LastOrDefault();
        After:
                return values == null || values.Length != 2 ||
                    values[0] is ContentTopicItem topicItem == false ||
                    values[1] is ContentTopicItem[] topicItems == false
                    ? true
                    : (object)(topicItem != topicItems.LastOrDefault());
        */

        /* Unmerged change from project 'SimplyScriptures.Maui(net8.0-windows10.0.19041.0)'
        Before:
                if (values == null || values.Length != 2 || 
                    values[0] is ContentTopicItem topicItem == false ||
                    values[1] is ContentTopicItem[] topicItems == false)
                {
                    return true;
                }

                return topicItem != topicItems.LastOrDefault();
        After:
                return values == null || values.Length != 2 ||
                    values[0] is ContentTopicItem topicItem == false ||
                    values[1] is ContentTopicItem[] topicItems == false
                    ? true
                    : (object)(topicItem != topicItems.LastOrDefault());
        */
        return values == null || values.Length != 2 ||
            values[0] is ContentTopicItem topicItem == false ||
            values[1] is ContentTopicItem[] topicItems == false
            ? true
            : (object)(topicItem != topicItems.LastOrDefault());
*/

/* Unmerged change from project 'SimplyScriptures.Maui(net8.0-windows10.0.19041.0)'
Before:
/* Unmerged change from project 'SimplyScriptures.Maui(net8.0-ios)'
Before:
        if (values == null || values.Length != 2 || 
            values[0] is ContentTopicItem topicItem == false ||
            values[1] is ContentTopicItem[] topicItems == false)
        {
            return true;
        }

        return topicItem != topicItems.LastOrDefault();
After:
        return values == null || values.Length != 2 ||
            values[0] is ContentTopicItem topicItem == false ||
            values[1] is ContentTopicItem[] topicItems == false
            ? true
            : (object)(topicItem != topicItems.LastOrDefault());
*/

/* Unmerged change from project 'SimplyScriptures.Maui(net8.0-maccatalyst)'
Before:
        if (values == null || values.Length != 2 || 
            values[0] is ContentTopicItem topicItem == false ||
            values[1] is ContentTopicItem[] topicItems == false)
        {
            return true;
        }

        return topicItem != topicItems.LastOrDefault();
After:
        return values == null || values.Length != 2 ||
            values[0] is ContentTopicItem topicItem == false ||
            values[1] is ContentTopicItem[] topicItems == false
            ? true
            : (object)(topicItem != topicItems.LastOrDefault());
*/

/* Unmerged change from project 'SimplyScriptures.Maui(net8.0-windows10.0.19041.0)'
Before:
        if (values == null || values.Length != 2 || 
            values[0] is ContentTopicItem topicItem == false ||
            values[1] is ContentTopicItem[] topicItems == false)
        {
            return true;
        }

        return topicItem != topicItems.LastOrDefault();
After:
        return values == null || values.Length != 2 ||
            values[0] is ContentTopicItem topicItem == false ||
            values[1] is ContentTopicItem[] topicItems == false
            ? true
            : (object)(topicItem != topicItems.LastOrDefault());
*/
        if (values == null || values.Length != 2 ||
            values[0] is ContentTopicItem topicItem == false ||
            values[1] is ContentTopicItem[] topicItems == false)
        {
            return true;
        }

        return topicItem != topicItems.LastOrDefault();
After:
        /* Unmerged change from project 'SimplyScriptures.Maui(net8.0-ios)'
        Before:
                if (values == null || values.Length != 2 || 
                    values[0] is ContentTopicItem topicItem == false ||
                    values[1] is ContentTopicItem[] topicItems == false)
                {
                    return true;
                }

                return topicItem != topicItems.LastOrDefault();
        After:
                return values == null || values.Length != 2 ||
                    values[0] is ContentTopicItem topicItem == false ||
                    values[1] is ContentTopicItem[] topicItems == false
                    ? true
                    : (object)(topicItem != topicItems.LastOrDefault());
        */

        /* Unmerged change from project 'SimplyScriptures.Maui(net8.0-maccatalyst)'
        Before:
                if (values == null || values.Length != 2 || 
                    values[0] is ContentTopicItem topicItem == false ||
                    values[1] is ContentTopicItem[] topicItems == false)
                {
                    return true;
                }

                return topicItem != topicItems.LastOrDefault();
        After:
                return values == null || values.Length != 2 ||
                    values[0] is ContentTopicItem topicItem == false ||
                    values[1] is ContentTopicItem[] topicItems == false
                    ? true
                    : (object)(topicItem != topicItems.LastOrDefault());
        */

        /* Unmerged change from project 'SimplyScriptures.Maui(net8.0-windows10.0.19041.0)'
        Before:
                if (values == null || values.Length != 2 || 
                    values[0] is ContentTopicItem topicItem == false ||
                    values[1] is ContentTopicItem[] topicItems == false)
                {
                    return true;
                }

                return topicItem != topicItems.LastOrDefault();
        After:
                return values == null || values.Length != 2 ||
                    values[0] is ContentTopicItem topicItem == false ||
                    values[1] is ContentTopicItem[] topicItems == false
                    ? true
                    : (object)(topicItem != topicItems.LastOrDefault());
        */
        return values == null || values.Length != 2 ||
            values[0] is ContentTopicItem topicItem == false ||
            values[1] is ContentTopicItem[] topicItems == false
            ? true
            : (object)(topicItem != topicItems.LastOrDefault());
*/
        /* Unmerged change from project 'SimplyScriptures.Maui(net8.0-ios)'
        Before:
                if (values == null || values.Length != 2 || 
                    values[0] is ContentTopicItem topicItem == false ||
                    values[1] is ContentTopicItem[] topicItems == false)
                {
                    return true;
                }

                return topicItem != topicItems.LastOrDefault();
        After:
                return values == null || values.Length != 2 ||
                    values[0] is ContentTopicItem topicItem == false ||
                    values[1] is ContentTopicItem[] topicItems == false
                    ? true
                    : (object)(topicItem != topicItems.LastOrDefault());
        */

        /* Unmerged change from project 'SimplyScriptures.Maui(net8.0-maccatalyst)'
        Before:
                if (values == null || values.Length != 2 || 
                    values[0] is ContentTopicItem topicItem == false ||
                    values[1] is ContentTopicItem[] topicItems == false)
                {
                    return true;
                }

                return topicItem != topicItems.LastOrDefault();
        After:
                return values == null || values.Length != 2 ||
                    values[0] is ContentTopicItem topicItem == false ||
                    values[1] is ContentTopicItem[] topicItems == false
                    ? true
                    : (object)(topicItem != topicItems.LastOrDefault());
        */

        /* Unmerged change from project 'SimplyScriptures.Maui(net8.0-windows10.0.19041.0)'
        Before:
                if (values == null || values.Length != 2 || 
                    values[0] is ContentTopicItem topicItem == false ||
                    values[1] is ContentTopicItem[] topicItems == false)
                {
                    return true;
                }

                return topicItem != topicItems.LastOrDefault();
        After:
                return values == null || values.Length != 2 ||
                    values[0] is ContentTopicItem topicItem == false ||
                    values[1] is ContentTopicItem[] topicItems == false
                    ? true
                    : (object)(topicItem != topicItems.LastOrDefault());
        */
        if (values == null || values.Length != 2 ||
            values[0] is ContentTopicItem topicItem == false ||
            values[1] is ContentTopicItem[] topicItems == false)
        {
            return true;
        }

        return topicItem != topicItems.LastOrDefault();
    }

    public static object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        return [Binding.DoNothing];
    }
}
