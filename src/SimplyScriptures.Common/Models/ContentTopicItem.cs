using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;
using SimplyScriptures.Common.Enums;
using SimplyScriptures.Common.Extensions;
using SimplyScriptures.Common.Html;
using SimplyScriptures.Common.Html.Models;

namespace SimplyScriptures.Common.Models;

public class ContentTopicItem
{
    public ScriptureBook Book { get; set; } = ScriptureBook.None;
    public string Text { get; set; } = "";
    public string Verse { get; set; } = "";
    public FormattedTextItem[] FormattedItems { get; set; } = Array.Empty<FormattedTextItem>();

    public void BuildFormattedItems()
    {
        if (FormattedItems.Length == 0)
        {
            var formatter = new HtmlFormatter();
            FormattedItems = formatter.ConvertToFormattedText(Text);
        }
    }

    public string ConvertToFullHtml()
    {
        var processedText = (Text ?? "")
            .Replace("\r\n", "<br />")
            .Replace("\n", "<br />")
            .Replace("<in/>", "<span style='margin-left: 2em;'></span>")
            .Replace("<in />", "<span style='margin-left: 2em;'></span>")
            .Replace("<bq>", "<blockquote>")
            .Replace("</bq>", "</blockquote>");

        // Force uppercase
        return char.ToUpper(processedText[0]) + processedText.Substring(1);
    }

    public string ConvertToFormattedText()
    {
        var processedText = (Text ?? "")
            .Replace("<br />", "\r\n")
            .Replace("<br/>", "\r\n")
            .Replace("<in/>", "    ")
            .Replace("<in />", "    ")
            .Replace("<bq>", "\r\n    ") // Not sure how to support quote block
            .Replace("</bq>", "\r\n    ");

        // Force uppercase
        var bodyText = char.ToUpper(processedText[0]) + processedText.Substring(1);
        return $"{bodyText}\r\n- {Verse}";
    }
}
