using System.Globalization;
using SimplyScriptures.Common.Enums;
using SimplyScriptures.Common.Extensions;

namespace SimplyScriptures.Converters;

public class ScriptureBookToDisplayConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null || value is ScriptureBook book == false)
        {
            return "";
        }

        return book.ToDisplayString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
