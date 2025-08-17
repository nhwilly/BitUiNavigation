using Bit.BlazorUI;
using BitUiNavigation.Client.Pages.UserProfile;
using Microsoft.AspNetCore.Components;
using TimeWarp.State;

namespace BitUiNavigation.Client.Pages.Modals;

public abstract class ModalProviderBase : TimeWarpStateComponent, IModalProvider
{
    public abstract string QueryKey { get; }
    public abstract string DefaultSection { get; }
    public abstract string Width { get; }
    public abstract string Height { get; }

    protected abstract Dictionary<string, Type> SectionMap { get; }

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
        if (v.StartsWith('/')) v = v[1..];
        return v;
    }

    public abstract List<BitNavItem> BuildNavItems(NavigationManager nav, string queryKey);
}