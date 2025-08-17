using Bit.BlazorUI;
using Microsoft.AspNetCore.Components;

namespace BitUiNavigation.Client.Pages.Modals;

public interface IModalProvider
{
    /// <summary>The query-string key that activates this modal (e.g., "modal", "help").</summary>
    string QueryKey { get; }

    /// <summary>The section used when the key exists but has no value.</summary>
    string DefaultSection { get; }

    /// <summary>Host BitModal width/height (CSS values).</summary>
    string Width { get; }
    string Height { get; }

    /// <summary>Builds the BitNav items for this modal. Each item’s Url should include the query param.</summary>
    List<BitNavItem> BuildNavItems(NavigationManager nav, string queryKey);

    /// <summary>Maps the current section key to a RouteData (component type + parameters) for the right pane.</summary>
    RouteData BuildRouteData(string sectionKey);
    Task OnModalOpenedAsync(CancellationToken ct);
    Task OnModalOpeningAsync(CancellationToken ct);
}
