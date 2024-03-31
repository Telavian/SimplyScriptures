using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimplyScriptures.Common.Html.Models;
using SimplyScriptures.Common.Models;

namespace SimplyScriptures.Converters;

public class FormattedTextItemToFontAttributesConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || value is FormattedTextItem item == false)
        {
            return FontAttributes.None;
        }

        var result = FontAttributes.None;

        if (item.IsBold)
        {
            result |= FontAttributes.Bold;
        }

        if (item.IsItalic)
        {
            result |= FontAttributes.Italic;
        }

        return result;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
