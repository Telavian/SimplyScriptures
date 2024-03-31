using System.Globalization;
using SimplyScriptures.Models.Interfaces;

namespace SimplyScriptures.Converters;

public class MenuContentItemToFontAttributeConverter : IValueConverter
{
    public static object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {


/* Unmerged change from project 'SimplyScriptures.Maui(net8.0-ios)'
Before:
/* Unmerged change from project 'SimplyScriptures.Maui(net8.0-ios)'
Before:
        if (value == null || value is IMenuContentItem item == false)
        {
            return FontAttributes.None;
        }

        return item.HasChildren 
            ? FontAttributes.Bold 
After:
        return value == null || value is IMenuContentItem item == false
            ? FontAttributes.None
            : item.HasChildren
            ? FontAttributes.Bold
*/

/* Unmerged change from project 'SimplyScriptures.Maui(net8.0-maccatalyst)'
Before:
        if (value == null || value is IMenuContentItem item == false)
        {
            return FontAttributes.None;
        }

        return item.HasChildren 
            ? FontAttributes.Bold 
After:
        return value == null || value is IMenuContentItem item == false
            ? FontAttributes.None
            : item.HasChildren
            ? FontAttributes.Bold
*/

/* Unmerged change from project 'SimplyScriptures.Maui(net8.0-windows10.0.19041.0)'
Before:
        if (value == null || value is IMenuContentItem item == false)
        {
            return FontAttributes.None;
        }

        return item.HasChildren 
            ? FontAttributes.Bold 
After:
        return value == null || value is IMenuContentItem item == false
            ? FontAttributes.None
            : item.HasChildren
            ? FontAttributes.Bold
*/
        if (value == null || value is IMenuContentItem item == false)
        {
            return FontAttributes.None;
        }

        return item.HasChildren
After:
        /* Unmerged change from project 'SimplyScriptures.Maui(net8.0-ios)'
        Before:
                if (value == null || value is IMenuContentItem item == false)
                {
                    return FontAttributes.None;
                }

                return item.HasChildren 
                    ? FontAttributes.Bold 
        After:
                return value == null || value is IMenuContentItem item == false
                    ? FontAttributes.None
                    : item.HasChildren
                    ? FontAttributes.Bold
        */

        /* Unmerged change from project 'SimplyScriptures.Maui(net8.0-maccatalyst)'
        Before:
                if (value == null || value is IMenuContentItem item == false)
                {
                    return FontAttributes.None;
                }

                return item.HasChildren 
                    ? FontAttributes.Bold 
        After:
                return value == null || value is IMenuContentItem item == false
                    ? FontAttributes.None
                    : item.HasChildren
                    ? FontAttributes.Bold
        */

        /* Unmerged change from project 'SimplyScriptures.Maui(net8.0-windows10.0.19041.0)'
        Before:
                if (value == null || value is IMenuContentItem item == false)
                {
                    return FontAttributes.None;
                }

                return item.HasChildren 
                    ? FontAttributes.Bold 
        After:
                return value == null || value is IMenuContentItem item == false
                    ? FontAttributes.None
                    : item.HasChildren
                    ? FontAttributes.Bold
        */
        return value == null || value is IMenuContentItem item == false
*/

/* Unmerged change from project 'SimplyScriptures.Maui(net8.0-maccatalyst)'
Before:
/* Unmerged change from project 'SimplyScriptures.Maui(net8.0-ios)'
Before:
        if (value == null || value is IMenuContentItem item == false)
        {
            return FontAttributes.None;
        }

        return item.HasChildren 
            ? FontAttributes.Bold 
After:
        return value == null || value is IMenuContentItem item == false
            ? FontAttributes.None
            : item.HasChildren
            ? FontAttributes.Bold
*/

/* Unmerged change from project 'SimplyScriptures.Maui(net8.0-maccatalyst)'
Before:
        if (value == null || value is IMenuContentItem item == false)
        {
            return FontAttributes.None;
        }

        return item.HasChildren 
            ? FontAttributes.Bold 
After:
        return value == null || value is IMenuContentItem item == false
            ? FontAttributes.None
            : item.HasChildren
            ? FontAttributes.Bold
*/

/* Unmerged change from project 'SimplyScriptures.Maui(net8.0-windows10.0.19041.0)'
Before:
        if (value == null || value is IMenuContentItem item == false)
        {
            return FontAttributes.None;
        }

        return item.HasChildren 
            ? FontAttributes.Bold 
After:
        return value == null || value is IMenuContentItem item == false
            ? FontAttributes.None
            : item.HasChildren
            ? FontAttributes.Bold
*/
        if (value == null || value is IMenuContentItem item == false)
        {
            return FontAttributes.None;
        }

        return item.HasChildren
After:
        /* Unmerged change from project 'SimplyScriptures.Maui(net8.0-ios)'
        Before:
                if (value == null || value is IMenuContentItem item == false)
                {
                    return FontAttributes.None;
                }

                return item.HasChildren 
                    ? FontAttributes.Bold 
        After:
                return value == null || value is IMenuContentItem item == false
                    ? FontAttributes.None
                    : item.HasChildren
                    ? FontAttributes.Bold
        */

        /* Unmerged change from project 'SimplyScriptures.Maui(net8.0-maccatalyst)'
        Before:
                if (value == null || value is IMenuContentItem item == false)
                {
                    return FontAttributes.None;
                }

                return item.HasChildren 
                    ? FontAttributes.Bold 
        After:
                return value == null || value is IMenuContentItem item == false
                    ? FontAttributes.None
                    : item.HasChildren
                    ? FontAttributes.Bold
        */

        /* Unmerged change from project 'SimplyScriptures.Maui(net8.0-windows10.0.19041.0)'
        Before:
                if (value == null || value is IMenuContentItem item == false)
                {
                    return FontAttributes.None;
                }

                return item.HasChildren 
                    ? FontAttributes.Bold 
        After:
                return value == null || value is IMenuContentItem item == false
                    ? FontAttributes.None
                    : item.HasChildren
                    ? FontAttributes.Bold
        */
        return value == null || value is IMenuContentItem item == false
*/

/* Unmerged change from project 'SimplyScriptures.Maui(net8.0-windows10.0.19041.0)'
Before:
/* Unmerged change from project 'SimplyScriptures.Maui(net8.0-ios)'
Before:
        if (value == null || value is IMenuContentItem item == false)
        {
            return FontAttributes.None;
        }

        return item.HasChildren 
            ? FontAttributes.Bold 
After:
        return value == null || value is IMenuContentItem item == false
            ? FontAttributes.None
            : item.HasChildren
            ? FontAttributes.Bold
*/

/* Unmerged change from project 'SimplyScriptures.Maui(net8.0-maccatalyst)'
Before:
        if (value == null || value is IMenuContentItem item == false)
        {
            return FontAttributes.None;
        }

        return item.HasChildren 
            ? FontAttributes.Bold 
After:
        return value == null || value is IMenuContentItem item == false
            ? FontAttributes.None
            : item.HasChildren
            ? FontAttributes.Bold
*/

/* Unmerged change from project 'SimplyScriptures.Maui(net8.0-windows10.0.19041.0)'
Before:
        if (value == null || value is IMenuContentItem item == false)
        {
            return FontAttributes.None;
        }

        return item.HasChildren 
            ? FontAttributes.Bold 
After:
        return value == null || value is IMenuContentItem item == false
            ? FontAttributes.None
            : item.HasChildren
            ? FontAttributes.Bold
*/
        if (value == null || value is IMenuContentItem item == false)
        {
            return FontAttributes.None;
        }

        return item.HasChildren
After:
        /* Unmerged change from project 'SimplyScriptures.Maui(net8.0-ios)'
        Before:
                if (value == null || value is IMenuContentItem item == false)
                {
                    return FontAttributes.None;
                }

                return item.HasChildren 
                    ? FontAttributes.Bold 
        After:
                return value == null || value is IMenuContentItem item == false
                    ? FontAttributes.None
                    : item.HasChildren
                    ? FontAttributes.Bold
        */

        /* Unmerged change from project 'SimplyScriptures.Maui(net8.0-maccatalyst)'
        Before:
                if (value == null || value is IMenuContentItem item == false)
                {
                    return FontAttributes.None;
                }

                return item.HasChildren 
                    ? FontAttributes.Bold 
        After:
                return value == null || value is IMenuContentItem item == false
                    ? FontAttributes.None
                    : item.HasChildren
                    ? FontAttributes.Bold
        */

        /* Unmerged change from project 'SimplyScriptures.Maui(net8.0-windows10.0.19041.0)'
        Before:
                if (value == null || value is IMenuContentItem item == false)
                {
                    return FontAttributes.None;
                }

                return item.HasChildren 
                    ? FontAttributes.Bold 
        After:
                return value == null || value is IMenuContentItem item == false
                    ? FontAttributes.None
                    : item.HasChildren
                    ? FontAttributes.Bold
        */
        return value == null || value is IMenuContentItem item == false
*/
        /* Unmerged change from project 'SimplyScriptures.Maui(net8.0-ios)'
        Before:
                if (value == null || value is IMenuContentItem item == false)
                {
                    return FontAttributes.None;
                }

                return item.HasChildren 
                    ? FontAttributes.Bold 
        After:
                return value == null || value is IMenuContentItem item == false
                    ? FontAttributes.None
                    : item.HasChildren
                    ? FontAttributes.Bold
        */

        /* Unmerged change from project 'SimplyScriptures.Maui(net8.0-maccatalyst)'
        Before:
                if (value == null || value is IMenuContentItem item == false)
                {
                    return FontAttributes.None;
                }

                return item.HasChildren 
                    ? FontAttributes.Bold 
        After:
                return value == null || value is IMenuContentItem item == false
                    ? FontAttributes.None
                    : item.HasChildren
                    ? FontAttributes.Bold
        */

        /* Unmerged change from project 'SimplyScriptures.Maui(net8.0-windows10.0.19041.0)'
        Before:
                if (value == null || value is IMenuContentItem item == false)
                {
                    return FontAttributes.None;
                }

                return item.HasChildren 
                    ? FontAttributes.Bold 
        After:
                return value == null || value is IMenuContentItem item == false
                    ? FontAttributes.None
                    : item.HasChildren
                    ? FontAttributes.Bold
        */
        if (value == null || value is IMenuContentItem item == false)
        {
            return FontAttributes.None;
        }

        return item.HasChildren
            ? FontAttributes.None
            : (object)(item.HasChildren
            ? FontAttributes.Bold
            : FontAttributes.None);
    }

    public static object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
