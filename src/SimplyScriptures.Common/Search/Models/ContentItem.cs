using SimplyScriptures.Common.Enums;

namespace SimplyScriptures.Common.Search.Models;

public class ContentItem
{
    public bool IsVisible { get; set; } = true;
    public ScriptureBook Book { get; set; } = ScriptureBook.None;
    public string Name { get; set; } = "";
    public string XPath { get; set; } = "";
    public ContentItem? Parent { get; set; }

    public ContentItem[] Children { get; set; } = Array.Empty<ContentItem>();

    public override string ToString()
    {
        return Name;
    }
}