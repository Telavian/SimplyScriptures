using CommunityToolkit.Mvvm.Input;

namespace SimplyScriptures.Models.Interfaces;

public interface IListItem
{
    IListItem Self { get; }
    bool IsSelected { get; set; }

    string Text { get; set; }
}
