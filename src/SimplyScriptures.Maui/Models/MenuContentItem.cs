using SimplyScriptures.Commands;
using SimplyScriptures.Models.Interfaces;

namespace SimplyScriptures.Models;

public class MenuContentItem<T>(T item) : ModelBase, IMenuContentItem
{
    IMenuContentItem IMenuContentItem.Self => Self;
    public MenuContentItem<T> Self => this;

    public T Item { get; } = item;

    #region HasChildren

    private bool _hasChildren;

    public bool HasChildren
    {
        get => _hasChildren;
        set => SetProperty(ref _hasChildren, value);
    }

    #endregion HasChildren

    #region IsExpanded

    private bool _isExpanded;

    public bool IsExpanded
    {
        get => _isExpanded;
        set => SetProperty(ref _isExpanded, value);
    }

    #endregion IsExpanded

    #region IsVisible

    private bool _isVisible;

    public bool IsVisible
    {
        get => _isVisible;
        set => SetProperty(ref _isVisible, value);
    }

    #endregion IsVisible

    #region IsSelected

    private bool _isSelected;

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    #endregion IsSelected

    #region TextHeader

    private string _textHeader = "";

    public string TextHeader
    {
        get => _textHeader;
        set => SetProperty(ref _textHeader, value);
    }

    #endregion TextHeader

    #region Text

    private string _text = "";

    public string Text
    {
        get => _text;
        set => SetProperty(ref _text, value);
    }

    #endregion Text

    #region Level

    private int _level;

    public int Level
    {
        get => _level;
        set => SetProperty(ref _level, value);
    }

    #endregion Level

    #region Parent

    IMenuContentItem? IMenuContentItem.Parent
    {
        get => Parent;
        set => Parent = (MenuContentItem<T>?)value;
    }

    private MenuContentItem<T>? _parent;

    public MenuContentItem<T>? Parent
    {
        get => _parent;
        set => SetProperty(ref _parent, value);
    }

    #endregion Parent

    #region AllChildren

    IReadOnlyList<IMenuContentItem> IMenuContentItem.AllChildren
    {
        get => AllChildren.ToArray();
        set => AllChildren = value.Select(x => (MenuContentItem<T>)x).ToArray();
    }

    private MenuContentItem<T>[] _allChildren = [];

    public MenuContentItem<T>[] AllChildren
    {
        get => _allChildren;
        set => SetProperty(ref _allChildren, value);
    }

    #endregion AllChildren

    #region Children

    IReadOnlyList<IMenuContentItem> IMenuContentItem.Children
    {
        get => Children.ToArray();
        set => Children = value.Select(x => (MenuContentItem<T>)x).ToArray();
    }

    private MenuContentItem<T>[] _children = [];

    public MenuContentItem<T>[] Children
    {
        get => _children;
        set => SetProperty(ref _children, value);
    }

    #endregion Children

    #region Action

    private Func<Task> _action = () => Task.CompletedTask;

    public Func<Task> Action
    {
        get => _action;
        set => SetProperty(ref _action, value);
    }

    #endregion Action

    #region Handler

    AsyncCommand<IMenuContentItem> IMenuContentItem.Handler => new AsyncCommand<IMenuContentItem>(async arg => Handler.Execute(arg));

    private AsyncCommand<MenuContentItem<T>> _handler = new(x => Task.CompletedTask);

    public AsyncCommand<MenuContentItem<T>> Handler
    {
        get => _handler;
        set => SetProperty(ref _handler, value);
    }

    #endregion Handler

    public override string ToString()
    {
        return Text;
    }

    public void ExpandItem()
    {
        IsExpanded = true;
        if (AllChildren.Length == 0 || Children.Length > 0)
        {
            return;
        }

        Children = AllChildren
            .Where(x => x.IsVisible)
            .ToArray();
    }

    public void CollapseItem()
    {
        IsExpanded = false;
    }

    public void AddChild(MenuContentItem<T> child)
    {
        AllChildren = AllChildren.Concat(new[] { child }).ToArray();

        if (child.IsVisible)
        {
            Children = Children.Concat(new[] { child }).ToArray();
        }
    }

    public void RemoveChild(MenuContentItem<T> child)
    {
        AllChildren = AllChildren.Except(new[] { child }).ToArray();
        Children = Children.Except(new[] { child }).ToArray();
    }
}
