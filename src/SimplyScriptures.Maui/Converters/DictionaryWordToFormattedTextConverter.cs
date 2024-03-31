using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimplyScriptures.Common.Models;
using SimplyScriptures.Models;
using SimplyScriptures.Services;

namespace SimplyScriptures.Converters;

public class DictionaryWordToFormattedTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is null || value is DictionaryWord item == false)
        {
            return "";
        }

        var html = item.ConvertToFullHtml()
            .Replace("&", "&#38;");

        return html;
        //var service = new HtmlFormattingService();
        //return service.ConvertToFormattedText(html);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
