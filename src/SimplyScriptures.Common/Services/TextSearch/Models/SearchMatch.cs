using SimplyScriptures.Common.Enums;
using SimplyScriptures.Common.Extensions;
using System.Runtime.CompilerServices;

namespace SimplyScriptures.Common.Services.TextSearch.Models;

public class SearchMatch
{
    public ScriptureBook Book { get; set; }

    public int Chapter { get; set; }

    public string FormattedText { get; set; } = "";

    public string[] Keywords { get; set; } = Array.Empty<string>();

    public SearchMode Mode { get; set; }

    public double Score { get; set; }

    public string Text { get; set; } = "";

    public int Verse { get; set; }

    public string XPath { get; set; } = "";

    public string BuildScriptureReference()
    {
        var displayString = Book.ToDisplayString();

        if (displayString == "About")
        {
            return displayString;
        }

        switch (Chapter)
        {
            case 1 when Verse == 1:
                return displayString;
            case > 0 when !displayString.StartsWith("Lecture") && !displayString.StartsWith("Section"):
                displayString = $"{displayString} {Chapter}";
                break;
        }

        switch (Verse)
        {
            case > 0:
                return $"{displayString}:{Verse}";
            default:
                return displayString;
        }
    }
}