using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using SimplyScriptures.Common.Extensions;
using SimplyScriptures.Services.ApplicationState.Interfaces;

namespace SimplyScriptures.Services.ApplicationState;

public class ApplicationStateService(ILocalStorageService localStorage) : IApplicationStateService
{
    #region Private Variables

    private readonly ILocalStorageService _localStorage = localStorage;

    #endregion

    #region IsOTVisible

    private bool _isOTVisible = true;

    public bool IsOTVisible
    {
        get => _isOTVisible;
        set => UpdateProperty(ref _isOTVisible, value,
            v => SaveSettingAsync(nameof(IsOTVisible), v));
    }

    #endregion IsOTVisible

    #region IsNTVisible

    private bool _isNTVisible = true;

    public bool IsNTVisible
    {
        get => _isNTVisible;
        set => UpdateProperty(ref _isNTVisible, value,
            v => SaveSettingAsync(nameof(IsNTVisible), v));
    }

    #endregion IsNTVisible

    #region IsBMVisible

    private bool _isBMVisible = true;

    public bool IsBMVisible
    {
        get => _isBMVisible;
        set => UpdateProperty(ref _isBMVisible, value,
            v => SaveSettingAsync(nameof(IsBMVisible), v));
    }

    #endregion IsBMVisible

    #region IsDCVisible

    private bool _isDCVisible = true;

    public bool IsDCVisible
    {
        get => _isDCVisible;
        set => UpdateProperty(ref _isDCVisible, value,
            v => SaveSettingAsync(nameof(IsDCVisible), v));
    }

    #endregion IsDCVisible

    #region IsDisplayInverted

    private bool _isDisplayInverted = false;

    public bool IsDisplayInverted
    {
        get => _isDisplayInverted;
        set => UpdateProperty(ref _isDisplayInverted, value,
            v => SaveSettingAsync(nameof(IsDisplayInverted), v));
    }

    #endregion IsDisplayInverted

    #region HasShownMobileUsageAlert

    private bool _hasShownMobileUsageAlert = false;

    public bool HasShownMobileUsageAlert
    {
        get => _hasShownMobileUsageAlert;
        set => UpdateProperty(ref _hasShownMobileUsageAlert, value, v => Task.CompletedTask);
    }

    #endregion HasShownMobileUsageAlert

    public async Task LoadCurrentStateAsync()
    {
        IsOTVisible = await GetSettingAsync(nameof(IsOTVisible), true);
        IsNTVisible = await GetSettingAsync(nameof(IsNTVisible), true);
        IsBMVisible = await GetSettingAsync(nameof(IsBMVisible), true);
        IsDCVisible = await GetSettingAsync(nameof(IsDCVisible), true);

        IsDisplayInverted = await GetSettingAsync(nameof(IsDisplayInverted), false);
    }

    #region Private Methods

    private static void UpdateProperty(ref bool property, bool value, Func<bool, Task> action)
    {
        if (property == value)
        {
            return;
        }

        property = value;
        action(value);
    }

    private async Task SaveSettingAsync(string name, bool value)
    {
        await _localStorage!.SetItemAsStringAsync(name, value.ToString())
                .ConfigureAwait(false);
    }

    private async Task<bool> GetSettingAsync(string name, bool defaultValue)
    {
        var value = await _localStorage!.GetItemAsStringAsync(name)
            .ConfigureAwait(false);

        var isValid = bool.TryParse(value, out var result);

        return isValid
            ? result
            : defaultValue;
    }

    #endregion
}
