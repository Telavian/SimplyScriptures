using SimplyScriptures.Commands;

namespace SimplyScriptures.Models.Interfaces;

public interface IMenuContentItem
{
    IMenuContentItem Self { get; }

    bool HasChildren { get; set; }
    bool IsExpanded { get; set; }
    bool IsVisible { get; set; }
    bool IsSelected { get; set; }

    string TextHeader { get; set; }
    string Text { get; set; }
    int Level { get; set; }
    IMenuContentItem? Parent { get; set; }
    IReadOnlyList<IMenuContentItem> AllChildren { get; set; }
    IReadOnlyList<IMenuContentItem> Children { get; set; }
    AsyncCommand<IMenuContentItem> Handler { get; }

    void CollapseItem();
    void ExpandItem();

    Func<Task> Action { get; }
}
