using System.Globalization;
using SimplyScriptures.Models.Interfaces;

namespace SimplyScriptures.Converters;

public class MenuContentItemToFontAttributeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || value is IMenuContentItem item == false)
        {
            return FontAttributes.None;
        }

        return item.HasChildren 
            ? FontAttributes.Bold 
            : FontAttributes.None;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
