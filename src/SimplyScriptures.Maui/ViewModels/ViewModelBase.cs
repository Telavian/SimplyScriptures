using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using SimplyScriptures.Commands;
using SimplyScriptures.Pages;

namespace SimplyScriptures.ViewModels;

public abstract class ViewModelBase : INotifyPropertyChanged
{
    private AsyncCommand? _showHomeAsyncCommand;
    public AsyncCommand ShowHomeAsyncCommand => _showHomeAsyncCommand ??= CreateAsyncCommand(ShowHomeAsync, "Unable to show home");

    private AsyncCommand? _showDisplayAsyncCommand;
    public AsyncCommand ShowDisplayAsyncCommand => _showDisplayAsyncCommand ??= CreateAsyncCommand(ShowDisplayAsync, "Unable to show display");

    private AsyncCommand? _showDictionaryAsyncCommand;
    public AsyncCommand ShowDictionaryAsyncCommand => _showDictionaryAsyncCommand ??= CreateAsyncCommand(ShowDictionaryAsync, "Unable to show dictionary");

    private AsyncCommand? _showTopicsAsyncCommand;    
    public AsyncCommand ShowTopicsAsyncCommand => _showTopicsAsyncCommand ??= CreateAsyncCommand(ShowTopicsAsync, "Unable to show topics");

    public event PropertyChangedEventHandler? PropertyChanged;

    #region Protected Methods

    protected async Task CopyItemToClipboardAsync(string text)
    {
        await Clipboard.SetTextAsync(text);
        await ViewModelBase.DisplayAlertAsync("Copy item", "Item copied to clipboard", "OK");
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
        if (SetPropertyItem(ref property, newValue, propertyName))
        {
            action(newValue);
            return true;
        }

        return false;
    }

    protected bool SetProperty<T>(ref T property, T newValue, [CallerMemberName] string? propertyName = null)
    {
        if (SetPropertyItem(ref property, newValue, propertyName))
        {
            return true;
        }

        return false;
    }

    protected bool SetProperty<T>(ref T property, T newValue, Func<T, Task> action, [CallerMemberName] string? propertyName = null)
    {
        if (SetPropertyItem(ref property, newValue, propertyName))
        {
            _ = action(newValue);
            return true;
        }

        return false;
    }

    protected bool SetProperty<T>(ref T[] property, T[] newValue, [CallerMemberName] string? propertyName = null)
    {
        return property != newValue && !property.SequenceEqual(newValue) && SetPropertyItem(ref property, newValue, propertyName);
    }

    protected bool SetProperty<T>(ref T[] property, T[] newValue, Action<T[]> action, [CallerMemberName] string? propertyName = null)
    {
        if (property == newValue || property.SequenceEqual(newValue))
        {
            return false;
        }

        if (SetPropertyItem(ref property, newValue, propertyName))
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

        if (SetPropertyItem(ref property, newValue, propertyName))
        {
            _ = action(newValue);
            return true;
        }

        return false;
    }

    private bool SetPropertyItem<T>(ref T? property, T? newValue, string? propertyName)
    {
        if (EqualityComparer<T>.Default.Equals(property, newValue))
        {
            return false;
        }

        property = newValue;
        if (PropertyChanged != null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        return true;
    }

    private bool SetPropertyItem<T>(ref T[] property, T[] newValue, string? propertyName)
    {
        if (EqualityComparer<T[]>.Default.Equals(property, newValue))
        {
            return false;
        }

        property = newValue;
        if (PropertyChanged != null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        return true;
    }

    protected void RaisePropertyChangedEvent(string name)
    {
        if (PropertyChanged != null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
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
        return Shell.Current.Dispatcher.DispatchAsync(action);
    }

    protected static Task<bool> DisplayAlertAsync(string title, string message, string accept, string cancel)
    {
        return DispatchAsync(async () => await Shell.Current.DisplayAlert(title, message, accept, cancel));
    }

    protected static Task DisplayAlertAsync(string title, string message, string cancel)
    {
        return DispatchAsync(async () => await Shell.Current.DisplayAlert(title, message, cancel));
    }

    protected static AsyncCommand CreateAsyncCommand(Func<Task> action, string message, [CallerMemberName] string? name = null)
    {
        var commandName = name;
        return new AsyncCommand(async () => await AttemptActionAsync(async () =>
                {
                    var timer = Stopwatch.StartNew();
                    Debug.WriteLine($"Executing {commandName}");

                    await action();

                    Debug.WriteLine($"{commandName}: {timer.ElapsedMilliseconds:N0} ms");
                }, message)
);
    }

    protected static AsyncCommand<T> CreateAsyncCommand<T>(Func<T?, Task> action, string message)
    {
        return new AsyncCommand<T>(async arg => await AttemptActionAsync(async () => await action(arg), message));
    }

    protected static async Task AttemptActionAsync(Func<Task> actionAsync, string message)
    {
        try
        {
            await actionAsync();
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
