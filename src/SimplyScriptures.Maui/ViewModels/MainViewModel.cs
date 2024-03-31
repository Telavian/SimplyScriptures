using CommunityToolkit.Mvvm.Input;
using SimplyScriptures.Common.Enums;
using SimplyScriptures.Common.Extensions;
using SimplyScriptures.Pages;

namespace SimplyScriptures.ViewModels;

public class MainViewModel : ViewModelBase
{
    #region Public Properties

    #region SearchText

    private readonly string _searchText = "";

    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    #endregion SearchText

    #region SearchAsyncCommand

    private AsyncRelayCommand? _searchAsyncCommand;

    public AsyncRelayCommand SearchAsyncCommand => _searchAsyncCommand ??= CreateAsyncCommand(SearchAsync, "Unable to search");

    #endregion SearchAsyncCommand

    #region SelectScriptureBookAsyncCommand

    private AsyncRelayCommand<ScriptureBook>? _selectScriptureBookAsyncCommand;

    public AsyncRelayCommand<ScriptureBook> SelectScriptureBookAsyncCommand
    {
        get
        {
            switch (_selectScriptureBookAsyncCommand)
            {
                case null:
                    {
                        return _selectScriptureBookAsyncCommand ??= CreateAsyncCommand<ScriptureBook>(SelectScriptureBookAsync, "Unable to select scripture book");
                    }

                default:
                    return _selectScriptureBookAsyncCommand;
            }
        }
    }

    #endregion SelectScriptureBookAsyncCommand

    #endregion

    #region Private Methods

    private Task SearchAsync()
    {
        return Shell.Current.GoToAsync(nameof(DisplayPage),
                new Dictionary<string, object>()
                {
                    ["SearchText"] = SearchText,
                    ["ScriptureBook"] = ScriptureBook.BM_About
                });
    }

    private static Task SelectScriptureBookAsync(ScriptureBook book)
    {
        return Shell.Current.GoToAsync(nameof(DisplayPage),
                new Dictionary<string, object>()
                {
                    ["SearchText"] = "",
                    ["ScriptureBook"] = book
                });
    }

    private static Task ShowTopicsAsync()
    {
        return Shell.Current.GoToAsync(nameof(TopicsPage));
    }

    private static Task ShowDictionaryAsync()
    {
        return Shell.Current.GoToAsync(nameof(DictionaryPage));
    }

    #endregion
}
