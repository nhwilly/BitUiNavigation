using Bit.BlazorUI;
using BitUiNavigation.Client.Pages.Modals;
using BitUiNavigation.Client.Pages.UserProfile;
using Microsoft.AspNetCore.Components;
using TimeWarp.State;

public abstract class ModalProviderBase : IModalProvider
{
    public abstract string QueryKey { get; }
    public abstract string DefaultSection { get; }
    public abstract string Width { get; }
    public abstract string Height { get; }
    protected readonly IStore Store;
    protected ModalProviderBase(IStore store)
    {
        Store = store;
    }
    protected abstract Dictionary<string, Type> SectionMap { get; }

    // 🟢 NEW: called by ModalHost when the modal is first opened.
    // Default does nothing; override in concrete provider to send init action.
    public virtual Task OnModalOpenedAsync(CancellationToken ct) => Task.CompletedTask;

    protected string BuildSectionUrl(NavigationManager nav, string section)
    {
        var currentPath = "/" + nav.ToBaseRelativePath(nav.Uri).Split('?')[0];
        var qs = System.Web.HttpUtility.ParseQueryString(new Uri(nav.Uri).Query);
        qs.Set(QueryKey, Normalize(section, DefaultSection));
        return $"{currentPath}?{qs}";
    }

    public virtual RouteData BuildRouteData(string sectionKey)
    {
        var key = Normalize(sectionKey, DefaultSection);
        var type = SectionMap.TryGetValue(key, out var t) ? t : typeof(NotFoundPanel);
        return new RouteData(type, new Dictionary<string, object?>());
    }

    protected static string Normalize(string? value, string defaultSection)
    {
        if (string.IsNullOrWhiteSpace(value)) return defaultSection;
        var v = value.Trim();
        return v.StartsWith('/') ? v[1..] : v;
    }

    public abstract List<BitNavItem> BuildNavItems(NavigationManager nav, string queryKey);
}
