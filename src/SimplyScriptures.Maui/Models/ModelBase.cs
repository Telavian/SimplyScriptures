using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SimplyScriptures.Models;

public class ModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected void SetProperty<T>(ref T property, T newValue, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(property, newValue))
        {
            return;
        }

        property = newValue;
        OnPropertyChanged(name);
    }

    protected void SetProperty<T>(ref T property, T newValue, Action<T> action, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(property, newValue))
        {
            return;
        }

        property = newValue;
        OnPropertyChanged(name);

        action(newValue);
    }

    protected void SetProperty<T>(ref T property, T newValue, Func<T, Task> action, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(property, newValue))
        {
            return;
        }

        property = newValue;
        OnPropertyChanged(name);

        _ = action(newValue);
    }
}
