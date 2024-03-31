﻿using System.Globalization;

namespace SimplyScriptures.Converters;

public class ScriptureBookToHtmlLinkConverter : IMultiValueConverter
{
    public static object Convert(object[]? values, Type targetType, object parameter, CultureInfo culture)
    {
        var url = "";

        if (values == null || values.Length != 1 || values[0] is string path == false)
        {
            url = App.IsDarkTheme == false
                ? "blank.html"
                : "blank-inverted.html";
        }
        else
        {
            var fullPath = $"{path}";

            if (fullPath.StartsWith("./"))
            {
                url = fullPath[2..];
            }
        }

        if (string.IsNullOrWhiteSpace(url))
        {
            url = App.IsDarkTheme == false
                ? "blank.html"
                : "blank-inverted.html";
        }

        url = url.Trim('/');
        if (DeviceInfo.Current.Platform == DevicePlatform.iOS)
        {
            url = Path.Combine(App.BaseDirectory, url);
            url = $"file://{url}";
        }

        if (File.Exists(url) == false)
        {
            Thread.Sleep(1000);
        }

        return url;
    }

    public static object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        return [Binding.DoNothing];
    }
}