using System.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using SimplyScriptures.Common.Enums;
using SimplyScriptures.Common.Extensions;
using SimplyScriptures.Pages.Common;
using SimplyScriptures.Services.ApplicationState.Interfaces;

namespace SimplyScriptures.Pages;

public class MainPageBase : ViewModelBase
{
    #region Private Variables

    [Inject]
    private NavigationManager? _navManager { get; set; }

    [Inject]
    private IApplicationStateService? _applicationState { get; set; }

    #endregion

    #region Public Properties

    #region IsDisplayInverted

    private bool _isDisplayInverted = false;

    public bool IsDisplayInverted
    {
        get => _isDisplayInverted;
        set => UpdateProperty(ref _isDisplayInverted, value,
            v => _applicationState!.IsDisplayInverted = v);
    }

    #endregion IsDisplayInverted

    #region SearchText

    private string _searchText = "";

    public string SearchText
    {
        get => _searchText;
        set => UpdateProperty(ref _searchText, value);
    }

    #endregion SearchText

    #region HandleSearchKeypressAsyncCommand

    private Func<KeyboardEventArgs, Task>? _handleSearchKeypressAsyncCommand;

    public Func<KeyboardEventArgs, Task> HandleSearchKeypressAsyncCommand => _handleSearchKeypressAsyncCommand ??= CreateEventCallbackAsyncCommand<KeyboardEventArgs>(HandleSearchKeypressAsync, "Unable to handle keypress");

    #endregion HandleSearchKeypressAsyncCommand

    #region SearchAsyncCommand

    private Func<Task>? _searchAsyncCommand;

    public Func<Task> SearchAsyncCommand => _searchAsyncCommand ??= CreateEventCallbackAsyncCommand(SearchAsync, "Unable to search");

    #endregion SearchAsyncCommand

    #region SelectScriptureBookAsyncCommand

    private Func<ScriptureBook, Task>? _selectScriptureBookAsyncCommand;

    public Func<ScriptureBook, Task> SelectScriptureBookAsyncCommand => _selectScriptureBookAsyncCommand ??= CreateEventCallbackAsyncCommand<ScriptureBook>(SelectScriptureBookAsync, "Unable to select book");

    #endregion SelectScriptureBookAsyncCommand

    #endregion

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync()
            ;

        SetPageState();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await _applicationState!.LoadCurrentStateAsync()
                ;

            SetPageState();

            await RefreshAsync()
                ;

            await AlertIfMobileAsync()
                ;
        }

        await base.OnAfterRenderAsync(firstRender)
            ;
    }

    #region Private Methods

    private Task HandleSearchKeypressAsync(KeyboardEventArgs args)
    {
        return args.Code is "Enter" or "Return" or "NumpadEnter" ? SearchAsyncCommand!() : Task.CompletedTask;
    }

    private Task SearchAsync()
    {
        var encoded = HttpUtility.UrlEncode(SearchText);
        _navManager!.NavigateTo($"/display?t={encoded}");

        return Task.CompletedTask;
    }

    private Task SelectScriptureBookAsync(ScriptureBook book)
    {
        _navManager!.NavigateTo($"/display?s={book}");
        return Task.CompletedTask;
    }

    private void SetPageState()
    {
        _isDisplayInverted = _applicationState!.IsDisplayInverted;
    }

    #endregion
}
