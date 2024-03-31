using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using Blazored.LocalStorage;
using CurrieTechnologies.Razor.Clipboard;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using SimplyScriptures.Common.Enums;
using SimplyScriptures.Common.Extensions;
using SimplyScriptures.Services.ApplicationState.Interfaces;

namespace SimplyScriptures.Pages.Common;

public partial class ViewModelBase : LayoutComponentBase
{
    #region Private Variables

    // http://detectmobilebrowsers.com/
    private static readonly Regex _browserDetectionRegex = MyRegex();
    private static readonly Regex _browserVersionDetectionRegex = MyRegex1();

    [Inject]
    private ILocalStorageService? _localStorage { get; set; }

    private bool _isUpdated = true;

    [Inject]
    private IJSRuntime? _jsRuntime { get; set; }

    [Inject]
    private ClipboardService? _clipboard { get; set; }

    [Inject]
    private IDialogService? _dialogService { get; set; }

    [Inject]
    private NavigationManager? _navigationManager { get; set; }

    [Inject]
    private IApplicationStateService? _applicationState { get; set; }

    #endregion

    #region Public Properties

    #region ShowHomeAsyncCommand

    private Func<Task>? _showHomeAsyncCommand;

    public Func<Task> ShowHomeAsyncCommand => _showHomeAsyncCommand ??= CreateEventCallbackAsyncCommand(ShowHomeAsync, "Unable to show home");

    #endregion ShowHomeAsyncCommand

    #region ShowScripturesAsyncCommand

    private Func<Task>? _showScripturesAsyncCommand;

    public Func<Task> ShowScripturesAsyncCommand => _showScripturesAsyncCommand ??= CreateEventCallbackAsyncCommand(ShowScripturesAsync, "Unable to show scriptures");

    #endregion ShowScipturesAsyncCommand

    #region ShowTopicsAsyncCommand

    private Func<Task>? _showTopicsAsyncCommand;

    public Func<Task> ShowTopicsAsyncCommand => _showTopicsAsyncCommand ??= CreateEventCallbackAsyncCommand(ShowTopicsAsync, "Unable to show topics");

    #endregion ShowTopicsAsyncCommand

    #region ShowDictionaryAsyncCommand

    private Func<Task>? _showDictionaryAsyncCommand;

    public Func<Task> ShowDictionaryAsyncCommand => _showDictionaryAsyncCommand ??= CreateEventCallbackAsyncCommand(ShowDictionaryAsync, "Unable to show dictionary");

    #endregion ShowDictionaryAsyncCommand

    #endregion

    #region Protected Methods

    protected async Task AlertIfMobileAsync()
    {
        if (_applicationState!.HasShownMobileUsageAlert)
        {
            return;
        }

        try
        {
            var userAgent = await _jsRuntime!.InvokeAsync<string>("getUserAgent")
                .ConfigureAwait(false);

            if (_browserDetectionRegex.IsMatch(userAgent) || _browserVersionDetectionRegex.IsMatch(userAgent[..4]))
            {
                await InvokeAsync(async () =>
                    {
                        _applicationState.HasShownMobileUsageAlert = true;
                        await _dialogService!
                            .ShowMessageBox("App", "Site is optimized for desktop. Use app on mobile devices")
                            .ConfigureAwait(false);
                    })
                    ;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unable to check user-agent: {ex.Message}");
        }
    }

    protected async Task<bool> LoadBooleanSettingAsync(string name, bool defaultValue)
    {
        var value = await _localStorage!.GetItemAsStringAsync(name)
            .ConfigureAwait(false);

        var isValid = bool.TryParse(value, out var result);

        return isValid
            ? result
            : defaultValue;
    }

    protected async Task<double> LoadDoubleSettingAsync(string name, double defaultValue)
    {
        var value = await _localStorage!.GetItemAsStringAsync(name)
            .ConfigureAwait(false);

        var isValid = double.TryParse(value, out var result);

        return isValid
            ? result
            : defaultValue;
    }

    protected async Task SaveSettingAsync(string name, bool value)
    {
        await _localStorage!.SetItemAsStringAsync(name, value.ToString())
            .ConfigureAwait(false);
    }

    protected async Task SaveSettingAsync(string name, int value)
    {
        await _localStorage!.SetItemAsStringAsync(name, value.ToString())
            .ConfigureAwait(false);
    }

    protected async Task SaveSettingAsync(string name, double value)
    {
        await _localStorage!.SetItemAsStringAsync(name, value.ToString(CultureInfo.InvariantCulture))
            .ConfigureAwait(false);
    }

    protected override bool ShouldRender()
    {
        if (_isUpdated)
        {
            _isUpdated = false;
            return true;
        }

        return false;
    }

    protected MudBlazor.Color BuildBookColor(ScriptureBook book)
    {
        if (book.IsOldTestament())
        {
            return MudBlazor.Color.Primary;
        }

        if (book.IsNewTestament())
        {
            return MudBlazor.Color.Info;
        }

        if (book.IsBookOfMormon())
        {
            return MudBlazor.Color.Success;
        }

        if (book.IsDoctrineAndCovenants())
        {
            return MudBlazor.Color.Dark; // TODO: Better color???
        }

        throw new Exception($"Invalid book: {book}");
    }

    protected async Task CopyTextToClipboardAsync(string text)
    {
        var isSupported = await InvokeAsync(async () => await _clipboard!.IsSupportedAsync()
)
            ;

        if (isSupported == false)
        {
            return;
        }

        await InvokeAsync(async () => await _clipboard!.WriteTextAsync(text)
)
            ;

        await InvokeAsync(async () => await _dialogService!.ShowMessageBox("Copy item", "Item copied to clipboard")
)
            ;
    }

    protected void UpdateProperty<T>(ref T property, T newValue)
    {
        if (EqualityComparer<T>.Default.Equals(property, newValue))
        {
            return;
        }

        property = newValue;
        _isUpdated = true;
    }

    protected void UpdateProperty<T>(ref T property, T newValue, Action<T> action)
    {
        if (EqualityComparer<T>.Default.Equals(property, newValue))
        {
            return;
        }

        property = newValue;
        _isUpdated = true;

        action(newValue);
    }

    protected async Task<T?> InvokeAsync<T>(Func<Task<T>> action)
    {
        T? result = default;

        await InvokeAsync(async () => result = await action()
)
        ;

        return result;
    }

    protected Func<Task> CreateEventCallbackAsyncCommand(Func<Task> action, string message)
    {
        return async () =>
        {
            await AttemptActionAsync(async () => await action()
, message)
            ;

            await RefreshAsync()
                ;
        };
    }

    protected Func<T, Task> CreateEventCallbackAsyncCommand<T>(Func<T, Task> action, string message)
    {
        return async (T args) =>
        {
            await AttemptActionAsync(async () => await action(args)
, message)
            ;

            await RefreshAsync()
                ;
        };
    }

    protected Func<T1, T2, Task> CreateEventCallbackAsyncCommand<T1, T2>(Func<T1, T2, Task> action, string message)
    {
        return async (T1 t1, T2 t2) =>
        {
            await AttemptActionAsync(async () => await action(t1, t2)
, message)
            ;

            await RefreshAsync()
                ;
        };
    }

    protected Action<T> CreateEventCallbackCommand<T>(Action<T> action, string message)
    {
        return (T args) =>
        {
            AttemptAction(() => action(args), message);

            RefreshAsync()
                ;
        };
    }

    protected Action CreateEventCallbackCommand(Action action, string message)
    {
        return () =>
        {
            AttemptAction(() => action(), message);

            InvokeAsync(() => StateHasChanged());
        };
    }

    protected void AttemptAction(Action action, string message)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            if (_jsRuntime != null)
            {
                _ = _jsRuntime.InvokeAsync<object>("alert", $"{message}: {ex.Message}");
            }
        }

        RefreshAsync()
            ;
    }

    protected async Task AttemptActionAsync(Func<Task> actionAsync, string message)
    {
        try
        {
            await actionAsync()
                ;
        }
        catch (Exception ex)
        {
            if (_jsRuntime != null)
            {
                await _jsRuntime.InvokeAsync<object>("alert", $"{message}: {ex.Message}")
                    .ConfigureAwait(false);
            }
        }

        await RefreshAsync()
            ;
    }

    protected Task RefreshAsync()
    {
        _isUpdated = true;

        return InvokeAsync(StateHasChanged);
    }

    protected string ConvertBooleanToDisplay(bool value)
    {
        return value
            ? "display: block;"
            : "display: none;";
    }

    #endregion

    #region Private Methods

    private Task ShowHomeAsync()
    {
        _navigationManager!.NavigateTo("/");
        return Task.CompletedTask;
    }

    private Task ShowScripturesAsync()
    {
        _navigationManager!.NavigateTo("/display");
        return Task.CompletedTask;
    }

    private Task ShowTopicsAsync()
    {
        _navigationManager!.NavigateTo("/topics");
        return Task.CompletedTask;
    }

    private Task ShowDictionaryAsync()
    {
        _navigationManager!.NavigateTo("/dictionary");
        return Task.CompletedTask;
    }

    [GeneratedRegex(@"(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|mobile.+firefox|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows ce|xda|xiino", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled, "en-US")]
    private static partial Regex MyRegex();
    [GeneratedRegex(@"1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\-(n|u)|c55\/|capi|ccwa|cdm\-|cell|chtm|cldc|cmd\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\-s|devi|dica|dmob|do(c|p)o|ds(12|\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\-|_)|g1 u|g560|gene|gf\-5|g\-mo|go(\.w|od)|gr(ad|un)|haie|hcit|hd\-(m|p|t)|hei\-|hi(pt|ta)|hp( i|ip)|hs\-c|ht(c(\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\-(20|go|ma)|i230|iac( |\-|\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\/)|klon|kpt |kwc\-|kyo(c|k)|le(no|xi)|lg( g|\/(k|l|u)|50|54|\-[a-w])|libw|lynx|m1\-w|m3ga|m50\/|ma(te|ui|xo)|mc(01|21|ca)|m\-cr|me(rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\-2|po(ck|rt|se)|prox|psio|pt\-g|qa\-a|qc(07|12|21|32|60|\-[2-7]|i\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\-|oo|p\-)|sdk\/|se(c(\-|0|1)|47|mc|nd|ri)|sgh\-|shar|sie(\-|m)|sk\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\-|v\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\-|tdg\-|tel(i|m)|tim\-|t\-mo|to(pl|sh)|ts(70|m\-|m3|m5)|tx\-9|up(\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|yas\-|your|zeto|zte\-", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled, "en-US")]
    private static partial Regex MyRegex1();

    #endregion
}
