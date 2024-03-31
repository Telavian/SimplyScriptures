using SimplyScriptures.Common.Extensions;

namespace SimplyScriptures.Common.Models;

public class DictionaryWord
{
    public string Word { get; set; } = "";
    public string Definition { get; set; } = "";

    public string ConvertToFullHtml()
    {
        var html = Definition?.Trim() ?? "";

        html = ReplaceCurrentWord(html);
        html = ReplaceNumbers(html)
            .Replace("\r\n", "<br /><br />")
            .Replace("\n", "<br /><br />");

        html = ReplacePartsOfSpeech(html);

        return html;
    }

    #region Private Methods

    private string ReplaceCurrentWord(string html)
    {
        var wordRepresentation = "";
        var index = html.IndexOf(',');
        if (index != -1)
        {
            wordRepresentation = html.Substring(0, index);
        }

        if (wordRepresentation.Contains(' '))
        {
            wordRepresentation = Word;
        }

        if (string.IsNullOrWhiteSpace(wordRepresentation))
        {
            return html;
        }

        return html
            .ReplaceEntireWord(wordRepresentation, $"<strong>{wordRepresentation}</strong>");
    }

    private static string ReplacePartsOfSpeech(string html)
    {
        return html
            .Replace(" n. ", " <em>noun</em> ", StringComparison.OrdinalIgnoreCase)
            .Replace(" a. ", " <em>adjective</em> ", StringComparison.OrdinalIgnoreCase)
            .Replace(" prep. ", " <em>preposition</em> ", StringComparison.OrdinalIgnoreCase)
            .Replace(" adv. ", " <em>adverb</em> ", StringComparison.OrdinalIgnoreCase)
            .Replace(" v.t. ", " <em>verb transitive</em> ", StringComparison.OrdinalIgnoreCase)
            .Replace(" pp. ", " <em>participle passive</em> ", StringComparison.OrdinalIgnoreCase)
            .Replace(" ppr. ", " <em>participle present tense</em> ", StringComparison.OrdinalIgnoreCase)
            .Replace(" v.t. ", " <em>transitive verb</em> ", StringComparison.OrdinalIgnoreCase)
            .Replace(" v.i. ", " <em>intransitive verb</em> ", StringComparison.OrdinalIgnoreCase);
    }

    private static string ReplaceNumbers(string html)
    {
        var index = 0;
        while (index != -1)
        {
            index = html.IndexOf('\n', index);

            if (index == -1)
            {
                continue;
            }

            if (char.IsDigit(html[index + 1]))
            {
                var end = html.IndexOf('.', index);
                html = html
                    .Insert(end, "</strong>")
                    .Insert(index, "<strong>");
                index = end;
            }
            else
            {
                index++;
            }
        }

        return html;
    }

    #endregion
}
