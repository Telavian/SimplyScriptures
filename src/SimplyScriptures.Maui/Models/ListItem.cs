using SimplyScriptures.Models.Interfaces;

namespace SimplyScriptures.Models;

public class ListItem<T> : ModelBase, IListItem
{
    IListItem IListItem.Self => Self;
    public ListItem<T> Self => this;

    public T Item { get; }

    #region IsSelected

    private bool _isSelected;

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    #endregion IsSelected

    #region Text

    private string _text = "";

    public string Text
    {
        get => _text;
        set => SetProperty(ref _text, value);
    }

    #endregion Text

    public ListItem(T item, string text)
    {
        Item = item;
        Text = text;
    }

    public override string ToString()
    {
        return Text;
    }
}
