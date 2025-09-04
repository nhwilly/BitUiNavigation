using BitUiNavigation.Client.Pages.Modal.Helpers;

namespace BitUiNavigation.Client.Pages.Modal.Abstract;

public interface IModalProvider
{
    string ProviderName { get; }
    string DefaultPanel { get; }
    string Width { get; }
    string Height { get; }
    string MinWidth { get; }
    string MaxWidth { get; }
    string InstanceName { get; }

    AutoSaveSupportResult AutoSaveSupportResult { get; }

    IReadOnlyList<string> ExpectedPanelKeys { get; }

    Task BuildNavSections(NavigationManager nav, CancellationToken ct);

    /// <summary>
    /// Maps the current panel key to a RouteData (component type + parameters) for the right panel.
    /// </summary>
    RouteData BuildRouteData(string panelKey);
    Task OnModalOpenedAsync(CancellationToken ct);
    Task OnModalOpeningAsync(CancellationToken ct);

    Task<bool> CanCloseAsync(CancellationToken ct);

    Task<(bool IsValid, string generalMessage, IReadOnlyList<string> Messages)> ValidateProviderAsync(CancellationToken ct);

    void DecorateCustomNavItemsWithValidationIndicators(List<CustomNavItem> items);

    bool HasUnsavedChanges { get; }

    Task ClearState(CancellationToken ct);

}