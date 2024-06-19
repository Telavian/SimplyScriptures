using System.IO.Compression;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using static Lucene.Net.Queries.Function.ValueSources.MultiFunction;

namespace SimplyScriptures.Common.Extensions;

public static partial class StringExtensions
{
    /// <summary>
    /// Limits string to closest word boundary after max length
    /// </summary>
    public static string LimitTo(this string? value, int limit)
    {
        if (value == null)
        {
            return "";
        }

        if (value.Length < limit)
        {
            return value;
        }

        var end = value.IndexOf(' ', limit);

        return end == -1
            ? value
            : value[..end]
                .Trim();
    }

    public static bool IsNumber(this string text)
    {
        return int.TryParse(text, out _);
    }

    public static bool IsAlpha(this string text)
    {
        return text
            .All(x => char.IsLetter(x) || char.IsWhiteSpace(x));
    }

    public static bool IsRegexMatch(this string text, string regex)
    {
        return new Regex(regex, RegexOptions.IgnoreCase).IsMatch(text);
    }

    public static bool Matches(this string text, string[] matches, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
    {
        return matches.Any(x => string.Equals(text, x, comparison));
    }

    public static int GetDamerauLevenshteinDistance(this string source, string target)
    {
        source = source.ToLower();
        target = target.ToLower();

        var bounds = new { Height = source.Length + 1, Width = target.Length + 1 };

        var matrix = new int[bounds.Height, bounds.Width];

        for (var height = 0; height < bounds.Height; height++)
        {
            matrix[height, 0] = height;
        };
        for (var width = 0; width < bounds.Width; width++)
        {
            matrix[0, width] = width;
        };

        for (var height = 1; height < bounds.Height; height++)
        {
            for (var width = 1; width < bounds.Width; width++)
            {
                var cost = (source[height - 1] == target[width - 1]) ? 0 : 1;
                var insertion = matrix[height, width - 1] + 1;
                var deletion = matrix[height - 1, width] + 1;
                var substitution = matrix[height - 1, width - 1] + cost;

                var distance = Math.Min(insertion, Math.Min(deletion, substitution));

                if (height > 1 && width > 1 && source[height - 1] == target[width - 2] && source[height - 2] == target[width - 1])
                {
                    distance = Math.Min(distance, matrix[height - 2, width - 2] + cost);
                }

                matrix[height, width] = distance;
            }
        }

        return matrix[bounds.Height - 1, bounds.Width - 1];
    }

    public static int NextWordIndex(this string text, int index)
    {
        if (index >= text.Length - 1)
        {
            return text.Length - 1;
        }

        index++;

        while (index < text.Length - 1)
        {
            if (char.IsWhiteSpace(text[index]))
            {
                index++;
            }
            else
            {
                break;
            }
        }

        while (index < text.Length - 1 && char.IsLetterOrDigit(text[index]))
        {
            index++;
        }

        return index;
    }

    public static int PreviousWordIndex(this string text, int index)
    {
        if (index == 0)
        {
            return 0;
        }

        index--;

        while (index > 0)
        {
            if (char.IsWhiteSpace(text[index]))
            {
                index--;
            }
            else
            {
                break;
            }
        }

        while (index > 0 && char.IsLetterOrDigit(text[index]))
        {
            index--;
        }

        return index;
    }

    public static string ReplaceEntireWord(this string text, string word, string replacement)
    {
        var pattern = $@"\b{Regex.Escape(word)}\b";
        return Regex.Replace(text, pattern, replacement, RegexOptions.IgnoreCase);
    }

    public static string SeparateAlphaNumeric(this string? text)
    {
        text ??= "";

        return MyRegex().Replace(text, " $& ").Trim();
    }

    public static string EncodeUrlString(this string text)
    {
        var bytes = Encoding.UTF8.GetBytes(text);
        var result = Convert.ToBase64String(bytes);

        return System.Web.HttpUtility.UrlEncode(result);
    }

    public static string DecodeUrlString(this string text)
    {
        var result = System.Web.HttpUtility.UrlDecode(text);
        var bytes = System.Convert.FromBase64String(result);
        return Encoding.UTF8.GetString(bytes);
    }

    public static int ToInt32(this string text)
    {
        return int.Parse(text);
    }

    public static int CountSubstringOccurrences(this string text, string value, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
    {
        var count = 0;
        var index = text.IndexOf(value, 0, comparison);

        while (index != -1)
        {
            index = text.IndexOf(value, index + value.Length, comparison);
            count++;
        }

        return count;
    }

    public static string UnescapeJson(this string text)
    {
        while (true)
        {
            var startLength = text.Length;
            text = text.Replace(@"\""", @"""");

            if (startLength == text.Length)
            {
                return text
                    .Trim('\\')
                    .Trim('"');
            }
        }
    }

    public static async Task<string> CompressTextAsync(this string text)
    {
        var bytes = Encoding.Unicode.GetBytes(text);
        await using var input = new MemoryStream(bytes);
        await using var output = new MemoryStream();
        await using var stream = new GZipStream(output, CompressionLevel.Optimal);

        await input.CopyToAsync(stream);
        await stream.FlushAsync();

        return Convert.ToBase64String(output.ToArray());
    }

    public static string CompressText(this string text)
    {
        var bytes = Encoding.Unicode.GetBytes(text);
        using var input = new MemoryStream(bytes);
        using var output = new MemoryStream();
        using var stream = new GZipStream(output, CompressionLevel.Optimal);

        input.CopyTo(stream);
        stream.Flush();

        return Convert.ToBase64String(output.ToArray());
    }

    public static async Task<string> DecompressTextAsync(this string text)
    {
        var bytes = Convert.FromBase64String(text);
        await using var input = new MemoryStream(bytes);
        await using var output = new MemoryStream();
        await using var stream = new GZipStream(input, CompressionMode.Decompress);
        await stream.CopyToAsync(output);
        await output.FlushAsync();

        return Encoding.Unicode.GetString(output.ToArray());
    }

    public static string ToInverseColor(this string color)
    {
        if (color == "transparent")
        {
            return color;
        }

        var r = Convert.ToByte(color.Substring(1, 2), 16);
        var g = Convert.ToByte(color.Substring(3, 2), 16);
        var b = Convert.ToByte(color.Substring(5, 2), 16);

        r = (byte)(1 - r);
        g = (byte)(1 - g);
        b = (byte)(1 - b);

        return $"#{r:X2}{g:X2}{b:X2}";
    }

    public static string StringJoin<T>(this IEnumerable<T> items, char separator)
    {
        return string.Join(separator, items);
    }

    public static string StringJoin<T>(this IEnumerable<T> items, string separator)
    {
        return string.Join(separator, items);
    }

    public static string TrimText(this string? text, Func<char, bool> allowed)
    {
        return text.TrimTextToParts(allowed)
            .StringJoin(" ");
    }

    public static string[] TrimTextToParts(this string? text, Func<char, bool> allowed)
    {
        text ??= "";

        var items = text
            .Select(x => allowed(x) ? x : ' ')
            .ToArray();
        var temp = new string(items);
        return temp.Split(Array.Empty<char>(), StringSplitOptions.RemoveEmptyEntries);
    }

    public static string TrimToAlphaNumeric(this string? text)
    {
        return text.TrimText(char.IsLetterOrDigit);
    }

    public static string[] TrimToAlphaNumericParts(this string? text)
    {
        return text.TrimTextToParts(char.IsLetterOrDigit);
    }

    public static string[] SplitAlphaNumeric(this string? text)
    {
        text ??= "";

        text = MyRegex().Replace(text, " $& ");
        return text.Split(Array.Empty<char>(), StringSplitOptions.RemoveEmptyEntries);
    }

    [GeneratedRegex(@"\d{1,}")]
    private static partial Regex MyRegex();
}
