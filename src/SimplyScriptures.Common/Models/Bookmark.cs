using SimplyScriptures.Common.Enums;

namespace SimplyScriptures.Common.Models;

public class Bookmark
{
    public Guid BookmarkId { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public ScriptureBook Book { get; set; }
    public int Location { get; set; }
}
