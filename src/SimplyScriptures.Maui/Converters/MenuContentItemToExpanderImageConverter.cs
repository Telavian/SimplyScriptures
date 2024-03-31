using System.Globalization;
using SimplyScriptures.Models.Interfaces;

namespace SimplyScriptures.Converters;

public class MenuContentItemToExpanderImageConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || value is bool isExpanded == false)
        {
            return Binding.DoNothing;
        }

        if (isExpanded)
        {
            return App.IsDarkTheme
                ? "down_dark.png"
                : "down_light.png";
        }

        return App.IsDarkTheme
            ? "right_dark.png"
            : "right_light.png";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
