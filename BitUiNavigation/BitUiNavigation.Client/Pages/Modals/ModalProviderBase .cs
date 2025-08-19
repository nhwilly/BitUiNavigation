using Bit.BlazorUI;
using BitUiNavigation.Client.Pages.UserProfile;
using Microsoft.AspNetCore.Components;
using TimeWarp.State;
using static BitUiNavigation.Client.Pages.Modals.UrlExtensions;

namespace BitUiNavigation.Client.Pages.Modals;
public abstract class ModalProviderBase : IModalProvider
{
    public abstract string ProviderKey { get; }
    public abstract string DefaultPanel { get; }
    public abstract string Width { get; }
    public abstract string Height { get; }
    protected readonly IStore Store;
    protected ModalProviderBase(IStore store)
    {
        Store = store;
    }
    protected abstract Dictionary<string, Type> PanelMap { get; }

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
    /// <param name="panel"></param>
    /// <returns></returns>
    protected string BuildPanelUrl(NavigationManager nav, string panel)
    {
        var currentPath = "/" + nav.ToBaseRelativePath(nav.Uri).Split('?')[0];
        var qs = System.Web.HttpUtility.ParseQueryString(new Uri(nav.Uri).Query);
        qs.Set(ProviderKey, Normalize(panel, DefaultPanel));
        return $"{currentPath}?{qs}";
    }

    public virtual RouteData BuildRouteData(string panelKey)
    {
        var key = Normalize(panelKey, DefaultPanel);
        var type = PanelMap.TryGetValue(key, out var t) ? t : typeof(NotFoundPanel);
        return new RouteData(type, new Dictionary<string, object?>());
    }

    /// <summary>
    /// Normalizes the panel URL value to ensure it is a valid path.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="defaultPanel"></param>
    /// <returns></returns>
    //protected static string Normalize(string? value, string defaultPanel)
    //{
    //    if (string.IsNullOrWhiteSpace(value)) return defaultPanel;
    //    var v = value.Trim();
    //    return v.StartsWith('/') ? v[1..] : v;
    //}

    /// <summary>
    /// Requires that all modal providers implement this method to build the navigation items.
    /// </summary>
    /// <param name="nav"></param>
    /// <param name="panelKey"></param>
    /// <returns></returns>
    public abstract List<BitNavItem> BuildNavItems(NavigationManager nav, string panelKey);
}
