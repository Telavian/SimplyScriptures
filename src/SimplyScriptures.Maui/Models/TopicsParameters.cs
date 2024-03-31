using SimplyScriptures.Common.Enums;

namespace SimplyScriptures.Models;

public class TopicsParameters
{
    public ScriptureBook? SelectedBook { get; set; }
    public string? SearchText { get; set; }
    public int? HighlightLocation { get; set; }
    public string[]? HighlightXPaths { get; set; }
}
