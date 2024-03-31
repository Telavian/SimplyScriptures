using SimplyScriptures.Common.Enums;

namespace SimplyScriptures.Pages.Models;

public class DisplayPageQueryParameters
{
    public ScriptureBook ScriptureParam { get; init; } = ScriptureBook.None;
    public string SearchText { get; init; } = "";
    public int LocationParam { get; init; } = 0;
    public string XPathsParam { get; init; } = "";

    public override string ToString()
    {
        return $"s={ScriptureParam}, t={SearchText}, l={LocationParam}, x={XPathsParam}";
    }
}
