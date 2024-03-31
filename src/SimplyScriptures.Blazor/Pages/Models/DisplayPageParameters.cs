using System.Text.Encodings.Web;
using System.Text.Json;
using System.Web;
using SimplyScriptures.Common.Enums;
using SimplyScriptures.Common.Extensions;

namespace SimplyScriptures.Pages.Models;

public class DisplayPageParameters
{
    public ScriptureBook Book { get; init; } = ScriptureBook.None;
    public string SearchText { get; init; } = "";
    public int Location { get; init; } = 0;
    public string[] XPaths { get; init; } = [];

    public string ConvertToLink(Uri root)
    {
        var query = $"s={Book}";
        if (Location > 0)
        {
            query += $"&l={Location}";
        }

        if (string.IsNullOrWhiteSpace(SearchText) == false)
        {
            query += $"&t={HttpUtility.UrlEncode(SearchText)}";
        }

        if (XPaths.Length > 0)
        {
            var json = XPaths.SerializeToJson();
            var compressed = json.CompressText();
            var encoded = HttpUtility.UrlEncode(compressed);

            query += $"&x={encoded}";
        }

        var url = new Uri(root, "display");
        return $"{url}?{query}";
    }
}
