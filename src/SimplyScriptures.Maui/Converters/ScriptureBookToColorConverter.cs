using System.Globalization;
using SimplyScriptures.Common.Enums;
using SimplyScriptures.Common.Extensions;

namespace SimplyScriptures.Converters;

public class ScriptureBookToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is null || value is ScriptureBook book == false)
        {
            return value;
        }

        if (book.IsOldTestament())
        {
            return Color.FromRgb(89, 74, 226);
        }

        if (book.IsNewTestament())
        {
            return Color.FromRgb(33, 150, 243);
        }

        if (book.IsBookOfMormon())
        {
            return Color.FromRgb(0, 200, 83);
        }

        if (book.IsDoctrineAndCovenants())
        {
            return Colors.DarkGray;
        }

        return Colors.White;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
