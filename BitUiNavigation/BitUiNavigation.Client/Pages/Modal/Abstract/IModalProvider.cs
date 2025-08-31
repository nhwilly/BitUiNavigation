using Bit.BlazorUI;
using BitUiNavigation.Client.Pages.Modal.Providers;
using Microsoft.AspNetCore.Components;

namespace BitUiNavigation.Client.Pages.Modal.Abstract;

public interface IModalProvider
{
    string ProviderName { get; }
    string DefaultPanel { get; }
    string Width { get; }
    string Height { get; }
    string MinWidth { get; }
    string MaxWidth { get; }

    IReadOnlyList<string> ExpectedPanelKeys { get; }

    List<NavSectionDetail> BuildCustomNavSections(NavigationManager nav);
    List<NavSectionDetail> NavSections { get; }

    /// <summary>
    /// Maps the current panel key to a RouteData (component type + parameters) for the right panel.
    /// </summary>
    RouteData BuildRouteData(string panelKey);
    Task OnModalOpenedAsync(CancellationToken ct);
    Task OnModalOpeningAsync(CancellationToken ct);

    Task<bool> CanCloseAsync(CancellationToken ct);

    Task<(bool IsValid, IReadOnlyList<string> Messages)> ValidateProviderAsync(CancellationToken ct);


}