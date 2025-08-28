using Bit.BlazorUI;
using Microsoft.AspNetCore.Components;

namespace BitUiNavigation.Client.Pages.Modals;

public interface IModalProvider
{
    string ProviderName { get; }
    string DefaultPanel { get; }
    IReadOnlyList<string> ExpectedPanelKeys { get; }
    string Width { get; }
    string Height { get; }
    string MinWidth { get; }

    List<NavSectionDetail> BuildCustomNavSections(NavigationManager nav);

    /// <summary>
    /// Maps the current panel key to a RouteData (component type + parameters) for the right panel.
    /// </summary>
    RouteData BuildRouteData(string panelKey);
    Task OnModalOpenedAsync(CancellationToken ct);
    Task OnModalOpeningAsync(CancellationToken ct);

    Task<bool> CanCloseAsync(CancellationToken ct);

    Task<(bool IsValid, IReadOnlyList<string> Messages)> ValidateProviderAsync(CancellationToken ct);


}