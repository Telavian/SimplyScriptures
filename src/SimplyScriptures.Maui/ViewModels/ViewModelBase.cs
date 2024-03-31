using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Dispatching;
using SimplyScriptures.Common.Extensions;
using SimplyScriptures.Pages;

namespace SimplyScriptures.ViewModels;

public abstract class ViewModelBase : ObservableObject
{
    #region Public Properties

    #region ShowHomeAsyncCommand

    private AsyncRelayCommand? _showHomeAsyncCommand;

    public AsyncRelayCommand ShowHomeAsyncCommand => _showHomeAsyncCommand ??= CreateAsyncCommand(ShowHomeAsync, "Unable to show home");

    #endregion ShowHomeAsyncCommand

    #region ShowDisplayAsyncCommand

    private AsyncRelayCommand? _showDisplayAsyncCommand;

    public AsyncRelayCommand ShowDisplayAsyncCommand => _showDisplayAsyncCommand ??= CreateAsyncCommand(ShowDisplayAsync, "Unable to show display");

    #endregion ShowDisplayAsyncCommand

    #region ShowDictionaryAsyncCommand

    private AsyncRelayCommand? _showDictionaryAsyncCommand;

    public AsyncRelayCommand ShowDictionaryAsyncCommand => _showDictionaryAsyncCommand ??= CreateAsyncCommand(ShowDictionaryAsync, "Unable to show dictionary");

    #endregion ShowDictionaryAsyncCommand

    #region ShowTopicsAsyncCommand

    private AsyncRelayCommand? _showTopicsAsyncCommand;

    public AsyncRelayCommand ShowTopicsAsyncCommand => _showTopicsAsyncCommand ??= CreateAsyncCommand(ShowTopicsAsync, "Unable to show topics");

    #endregion ShowTopicsAsyncCommand

    #endregion

    #region Protected Methods

    protected async Task CopyItemToClipboardAsync(string text)
    {
        await Clipboard.SetTextAsync(text)
            ;

        await ViewModelBase.DisplayAlertAsync("Copy item", "Item copied to clipboard", "OK")
            ;
    }

    protected static async Task<bool> LoadBooleanSettingAsync(string name, bool defaultValue)
    {
        await Task.Yield();
        return Preferences.Get(name, defaultValue);
    }

    protected static async Task<double> LoadDoubleSettingAsync(string name, double defaultValue)
    {
        await Task.Yield();
        return Preferences.Get(name, defaultValue);
    }

    protected static async Task SaveSettingAsync(string name, bool value)
    {
        await Task.Yield();
        Preferences.Set(name, value);
    }

    protected static async Task SaveSettingAsync(string name, int value)
    {
        await Task.Yield();
        Preferences.Set(name, value);
    }

    protected static async Task SaveSettingAsync(string name, double value)
    {
        await Task.Yield();
        Preferences.Set(name, value);
    }

    protected bool SetProperty<T>(ref T property, T newValue, Action<T> action, [CallerMemberName] string? propertyName = null)
    {
        if (base.SetProperty(ref property, newValue, propertyName))
        {
            action(newValue);
            return true;
        }

        return false;
    }

    protected bool SetProperty<T>(ref T property, T newValue, Func<T, Task> action, [CallerMemberName] string? propertyName = null)
    {
        if (base.SetProperty(ref property, newValue, propertyName))
        {
            _ = action(newValue)
                ;
            return true;
        }

        return false;
    }

    protected bool SetProperty<T>(ref T[] property, T[] newValue, [CallerMemberName] string? propertyName = null)
    {
        return property != newValue && !property.SequenceEqual(newValue) && base.SetProperty(ref property, newValue, propertyName);
    }

    protected bool SetProperty<T>(ref T[] property, T[] newValue, Action<T[]> action, [CallerMemberName] string? propertyName = null)
    {
        if (property == newValue || property.SequenceEqual(newValue))
        {
            return false;
        }

        if (base.SetProperty(ref property, newValue, propertyName))
        {
            action(newValue);
            return true;
        }

        return false;
    }

    protected bool SetProperty<T>(ref T[] property, T[] newValue, Func<T[], Task> action, [CallerMemberName] string? propertyName = null)
    {
        if (property == newValue || property.SequenceEqual(newValue))
        {
            return false;
        }

        if (base.SetProperty(ref property, newValue, propertyName))
        {
            _ = action(newValue)
                ;
            return true;
        }

        return false;
    }

    protected static Task DispatchAsync(Action action)
    {
        return Shell.Current.Dispatcher.DispatchAsync(action);
    }

    protected static Task DispatchAsync(Func<Task> action)
    {
        return Shell.Current.Dispatcher.DispatchAsync(action);
    }

    protected static Task<T> DispatchAsync<T>(Func<Task<T>> action)
    {
        return Shell.Current.Dispatcher.DispatchAsync(action)
            ;
    }

    protected static Task<bool> DisplayAlertAsync(string title, string message, string accept, string cancel)
    {
        return DispatchAsync(async () => await Shell.Current.DisplayAlert(title, message, accept, cancel)
)
        ;
    }

    protected static Task DisplayAlertAsync(string title, string message, string cancel)
    {
        return DispatchAsync(async () => await Shell.Current.DisplayAlert(title, message, cancel)
);
    }

    protected static AsyncRelayCommand CreateAsyncCommand(Func<Task> action, string message, [CallerMemberName] string? name = null)
    {
        var commandName = name;
        return new AsyncRelayCommand(async () => await AttemptActionAsync(async () =>
                {
                    var timer = Stopwatch.StartNew();
                    Debug.WriteLine($"Executing {commandName}");

                    await action()
                        ;

                    Debug.WriteLine($"{commandName}: {timer.ElapsedMilliseconds:N0} ms");
                }, message)
);
    }

    protected static AsyncRelayCommand<T> CreateAsyncCommand<T>(Func<T?, Task> action, string message)
    {
        return new AsyncRelayCommand<T>(async arg => await AttemptActionAsync(async () => await action(arg)
, message)
);
    }

    protected static async Task AttemptActionAsync(Func<Task> actionAsync, string message)
    {
        try
        {
            await actionAsync()
                ;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"{message}: {ex.Message}");
            // TODO: What to do?
        }
    }

    #endregion

    #region Private Methods

    private static Task ShowHomeAsync()
    {
        return Shell.Current.GoToAsync($"//{nameof(MainPage)}");
    }

    private static Task ShowDisplayAsync()
    {
        return Shell.Current.GoToAsync($"/{nameof(DisplayPage)}");
    }

    private static Task ShowDictionaryAsync()
    {
        return Shell.Current.GoToAsync($"/{nameof(DictionaryPage)}");
    }

    private static Task ShowTopicsAsync()
    {
        return Shell.Current.GoToAsync($"/{nameof(TopicsPage)}");
    }

    #endregion
}
