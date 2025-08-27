using Bit.BlazorUI;
using BitUiNavigation.Client.Pages.UserProfile;
using BitUiNavigation.Client.Services;
using Microsoft.AspNetCore.Components;
using TimeWarp.State;
using static BitUiNavigation.Client.Pages.Modals.UrlExtensions;

namespace BitUiNavigation.Client.Pages.Modals;

public abstract class ModalProviderBase : IModalProvider
{
    public abstract string ProviderName { get; }
    public abstract string DefaultPanel { get; }
    public abstract string Width { get; }
    public virtual string MinWidth => "350px";
    public abstract string Height { get; }

    protected readonly IStore Store;
    protected readonly ILogger _logger;

    protected ModalProviderBase(IStore store, ILogger logger)
    {
        Store = store;
        _logger = logger;
    }

    // Access central modal state
    protected ModalHostState HostState => Store.GetState<ModalHostState>();

    // Map from normalized panel key -> component type
    protected abstract Dictionary<string, Type> PanelMap { get; }
    /// <summary>
    /// Public, normalized keys that ModalHost can use.
    /// Normalization must match what you publish in ModalPanelBase / ModalContext.
    /// </summary>
    public virtual IReadOnlyList<string> ExpectedPanelKeys =>
        [.. PanelMap.Keys.Select(k => Normalize(k, DefaultPanel))];
    /// <summary>
    /// Optional hook after the modal is opened.
    /// </summary>
    public virtual Task OnModalOpenedAsync(CancellationToken ct) => Task.CompletedTask;

    /// <summary>
    /// Optional hook before the modal is opened.
    /// </summary>
    public virtual Task OnModalOpeningAsync(CancellationToken ct) => Task.CompletedTask;

    /// <summary>
    /// Build nav items for this provider.
    /// </summary>
    //public abstract List<BitNavItem> BuildNavItems(NavigationManager nav);

    public abstract List<CustomNavItem> BuildCustomNavItems(NavigationManager nav);
    public abstract List<NavSectionDetail> BuildCustomNavSections(NavigationManager nav);

    /// <summary>
    /// Create a URL for a specific panel within this modal.
    /// </summary>
    protected string BuildPanelUrl(NavigationManager nav, string panelName)
    {
        var currentPath = "/" + nav.ToBaseRelativePath(nav.Uri).Split('?')[0];
        var qs = System.Web.HttpUtility.ParseQueryString(new Uri(nav.Uri).Query);
        qs.Set(ProviderName, Normalize(panelName, DefaultPanel));
        return $"{currentPath}?{qs}";
    }

    public virtual RouteData BuildRouteData(string panelName)
    {
        var key = Normalize(panelName, DefaultPanel);
        var type = PanelMap.TryGetValue(key, out var t) ? t : typeof(NotFoundPanel);
        return new RouteData(type, new Dictionary<string, object?>());
    }

    /// <summary>
    /// If true, panels that have never published validity will block closing.
    /// Default is false (missing = treated as valid).
    /// </summary>
    protected virtual bool MissingPanelValidityBlocksClose => false;

    public virtual Task<bool> CanCloseAsync(CancellationToken ct) => Task.FromResult(true);

    protected void DecorateCustomNavItemsWithValidationIndicators(List<CustomNavItem> items)
    {
        // Snapshot to avoid repeated state reads
        var host = Store.GetState<ModalHostState>();

        // Get this provider's panel-validity map, if any
        host.Validity.TryGetValue(ProviderName, out var perPanel);

        foreach (var item in items)
        {
            if (item is null || string.IsNullOrWhiteSpace(item.Key)) continue;

            var panelKey = UrlExtensions.Normalize(item.Key, DefaultPanel);

            // Safe: only enters when perPanel is not null AND the key exists.
            if (perPanel?.TryGetValue(panelKey, out var pv) == true && pv is { IsValid: false })
            {
                item.ValidationIconName = BitIconName.CriticalErrorSolid;

                var msg = pv.ErrorCount == 1
                    ? "a validation error"
                    : $"{pv.ErrorCount} validation errors";

                // Optional a11y/tooltip text
                item.Title = string.IsNullOrWhiteSpace(item.Title)
                    ? $"Has {msg}"
                    : $"{item.Title} has {msg}";

                item.AriaLabel = $"{item.Text} has {msg}";
            }
            else
            {
                // leave as-is for valid/unknown
                // If you want to clear or set an "ok" icon, do it here.
                // item.ValidationIconName = null;
            }
        }
    }


}
