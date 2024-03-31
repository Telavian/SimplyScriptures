using SimplyScriptures.Common.Html.Models;

namespace SimplyScriptures.Common.Html;

public class HtmlFormatter
{
    public FormattedTextItem[] ConvertToFormattedText(string html)
    {
        var segments = BreakIntoSegments(html);

        var items = new List<FormattedTextItem>();
        var indentationLevel = 0;

        for (var index = 0; index < segments.Length; index++)
        {
            var segment = segments[index];

            switch (segment.StartsWith('<'))
            {
                case false:
                    items.Add(new FormattedTextItem { IndentationLevel = indentationLevel, Text = segment, Index = items.Count });
                    break;
                default:
                    {
                        switch (segment)
                        {
                            case "<br/>":
                            case "<br />":
                                {
                                    var nextSegment = segments[index + 1];
                                    switch (nextSegment)
                                    {
                                        case "<br/>":
                                        case "<br />":
                                            items.Add(new FormattedTextItem { IndentationLevel = indentationLevel, Text = "", Index = items.Count });
                                            index++;
                                            break;
                                    }

                                    break;
                                }

                            case "<in/>":
                            case "<in />":
                                {
                                    // Indent next item
                                    var next = segments[index + 1];
                                    var amount = 1;

                                    switch (next)
                                    {
                                        case "<in/>":
                                        case "<in />":
                                            next = segments[index + 2];
                                            amount++;
                                            break;
                                    }

                                    items.Add(new FormattedTextItem { IndentationLevel = indentationLevel + amount, Text = next, Index = items.Count });
                                    index += amount;
                                    break;
                                }

                            case "<bq>":
                                indentationLevel++;
                                break;
                            case "</bq>":
                                indentationLevel--;
                                items.Add(new FormattedTextItem { IndentationLevel = indentationLevel, Text = "", Index = items.Count });
                                break;
                            case "<em>":
                                {
                                    var next = segments[index + 1];
                                    items.Add(new FormattedTextItem { IndentationLevel = indentationLevel, Text = next, IsItalic = true, Index = items.Count });
                                    index++;
                                    break;
                                }

                            case "<strong>":
                                {
                                    var next = segments[index + 1];
                                    items.Add(new FormattedTextItem { IndentationLevel = indentationLevel, Text = next, IsBold = true, Index = items.Count });
                                    index++;
                                    break;
                                }

                            case "</em>":
                            case "</strong>":
                                break;
                            default:
                                throw new Exception($"Unknown formatted tag '{segment}'");
                        }

                        break;
                    }
            }
        }

        return items
            .ToArray();
    }

    private static string[] BreakIntoSegments(string? html)
    {
        var text = (html ?? "")
            .Replace("\r\n", "<br />")
            .Replace("\n", "<br />");

        text = char.ToUpper(text[0]) + text.Substring(1);

        var index = 0;
        var segments = new List<string>();

        while (true)
        {
            var tagStart = text.IndexOf('<', index);
            string fragment;

            if (tagStart == -1)
            {
                fragment = text.Substring(index).Trim();
                if (string.IsNullOrWhiteSpace(fragment) == false)
                {
                    segments.Add(fragment);
                }

                break;
            }

            var tagEnd = text.IndexOf('>', tagStart);

            fragment = text.Substring(index, tagStart - index).Trim();

            if (string.IsNullOrWhiteSpace(fragment) == false)
            {
                segments.Add(fragment);
            }

            segments.Add(text.Substring(tagStart, tagEnd - tagStart + 1).Trim());
            index = tagEnd + 1;
        }

        return segments
            .ToArray();
    }
}
