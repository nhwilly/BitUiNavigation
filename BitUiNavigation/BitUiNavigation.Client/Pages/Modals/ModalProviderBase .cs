using Bit.BlazorUI;
using BitUiNavigation.Client.Pages.UserProfile;
using Microsoft.AspNetCore.Components;
using TimeWarp.State;
using static BitUiNavigation.Client.Pages.Modals.UrlExtensions;

namespace BitUiNavigation.Client.Pages.Modals;
public abstract class ModalProviderBase : IModalProvider
{
    public abstract string ProviderName { get; }

    public abstract string DefaultPanel { get; }
    public abstract string Width { get; }
    public abstract string Height { get; }
    protected readonly IStore Store;
    protected readonly ILogger _logger;
    protected ModalProviderBase(IStore store, IModalPanelRegistry registry, ILogger logger)
    {
        Store = store; PanelRegistry = registry;
        _logger = logger;
    }

    protected abstract Dictionary<string, Type> PanelMap { get; }

    public IModalPanelRegistry PanelRegistry { get; }// = new ModalPanelRegistry();

    /// <summary>
    /// Optional override to handle logic after the modal is opened.
    /// </summary>
    /// <param name="ct"></param>
    /// <returns></returns>
    /// 
    public virtual Task OnModalOpenedAsync(CancellationToken ct) => Task.CompletedTask;

    /// <summary>
    /// Optional override to handle logic before the modal is opened.
    /// </summary>
    /// <param name="ct"></param>
    /// <returns></returns>
    public virtual Task OnModalOpeningAsync(CancellationToken ct) => Task.CompletedTask;

    /// <summary>
    /// Creates a panel url that can be used by NavigationManager to 
    /// navigate to a specific panel in the modal.
    /// </summary>
    /// <param name="nav"></param>
    /// <param name="panelName"></param>
    /// <returns></returns>
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
    /// Requires that all modal providers implement this method to build the navigation items.
    /// </summary>
    /// <param name="nav"></param>
    /// <param name="panelName"></param>
    /// <returns></returns>
    public abstract List<BitNavItem> BuildNavItems(NavigationManager nav);
    public abstract List<CustomNavItem> BuildCustomNavItems(NavigationManager nav);

    public virtual async Task<bool> CanCloseAsync(CancellationToken ct)
    {
        var canClose = true;
        var lastKnown = PanelRegistry.LastKnownValidityByType;
        foreach (var kv in PanelMap) // kv.Value is the component Type
        {
            var panelType = kv.Value;
            var exists = lastKnown.TryGetValue(panelType, out var isValid);
            _logger.LogDebug("Last known validity state for Panel: {Name} Exists: {Exists} IsValid: {IsValid}", panelType.Name, exists, isValid);
            if (exists & !isValid)
            {
                _logger.LogWarning("Panel {PanelType} is invalid, cannot close modal", panelType.Name);
                if (!isValid) canClose = false; // block close
            }
            else
            {
                // If you want to require that the user visits every panel before closing,
                // uncomment the next line:
                // return Task.FromResult(false);
            }
        }
        return await Task.FromResult(canClose);

    }


    public Task<List<BitNavItem>> BuildNavItemsWithValidationAsync(NavigationManager nav, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
