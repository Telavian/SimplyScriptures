using SimplyScriptures.Common.Enums;

namespace SimplyScriptures.Common.Services.TextSearch.Models;

public class SearchItem
{
    public string Text { get; set; } = "";
    public string XPath { get; set; } = "";
    public ScriptureBook Book { get; set; } = ScriptureBook.None;
    public int Chapter { get; set; } = 0;
    public int Verse { get; set; } = 0;

    public override string ToString()
    {
        return $"T:{Text}, B:{Book}, C:{Chapter}, V:{Verse}, X:{XPath}";
    }
}
