namespace SimplyScriptures.Services.ApplicationState.Interfaces;

public interface IApplicationStateService
{
    bool IsDisplayInverted { get; set; }
    bool IsOTVisible { get; set; }
    bool IsNTVisible { get; set; }
    bool IsBMVisible { get; set; }
    bool IsDCVisible { get; set; }

    bool HasShownMobileUsageAlert { get; set; }

    Task LoadCurrentStateAsync();
}
