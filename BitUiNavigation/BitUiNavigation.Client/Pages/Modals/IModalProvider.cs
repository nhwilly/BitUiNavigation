using Bit.BlazorUI;
using Microsoft.AspNetCore.Components;

namespace BitUiNavigation.Client.Pages.Modals;

public interface IModalProvider
{
    /// <summary>The query-string key that activates this modal (e.g., "modal", "help").</summary>
    string ProviderName { get; }
    /// <summary>The section used when the key exists but has no value.</summary>
    string DefaultPanel { get; }

    /// <summary>Host BitModal width/height (CSS values).</summary>
    string Width { get; }
    string Height { get; }
    public IModalPanelRegistry? PanelRegistry { get; }
    /// <summary>Builds the BitNav items for this modal. Each item’s Url should include the query param.</summary>
    List<BitNavItem> BuildNavItems(NavigationManager nav);
    List<CustomNavItem> BuildCustomNavItems(NavigationManager nav);

    /// <summary>
    /// Maps the current panel key to a RouteData (component type + parameters) for the right panel.
    /// </summary>
    RouteData BuildRouteData(string panelKey);
    Task OnModalOpenedAsync(CancellationToken ct);
    Task OnModalOpeningAsync(CancellationToken ct);

    // 🔹 NEW: Ask provider if navigation is allowed (aggregates panels)
    Task<bool> CanCloseAsync(CancellationToken ct);

    // 🔹 NEW: Refresh nav items to show validation indicators
    Task<List<BitNavItem>> BuildNavItemsWithValidationAsync(NavigationManager nav, CancellationToken ct);
}