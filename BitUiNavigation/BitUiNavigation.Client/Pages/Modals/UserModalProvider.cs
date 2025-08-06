using Bit.BlazorUI;
using BitUiNavigation.Client.Pages.UserProfile;
using Microsoft.AspNetCore.Components;

namespace BitUiNavigation.Client.Pages.Modals;

public sealed class UserModalProvider : IModalProvider
{
    public string QueryKey => nameof(UserModalProvider);
    public string DefaultSection => nameof(UserMembershipsPanel);
    public string Width => "900px";
    public string Height => "640px";

    public List<BitNavItem> BuildNavItems(NavigationManager nav, string queryKey)
    {
        string url(string section)
        {
            var currentPath = "/" + nav.ToBaseRelativePath(nav.Uri).Split('?')[0];
            var qs = System.Web.HttpUtility.ParseQueryString(new Uri(nav.Uri).Query);
            qs.Set(queryKey, Normalize(section));
            return $"{currentPath}?{qs}";
        }

        return new()
    {
        new() { Key = nameof(UserMembershipsPanel), Text = "Memberships", Url = url(nameof(UserMembershipsPanel)) },
        new() { Key = nameof(UserProfilePanel),     Text = "Profile",     Url = url(nameof(UserProfilePanel)) }
    };
    }


    public RouteData BuildRouteData(string sectionKey)
    {
        var map = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
        {
            [nameof(UserMembershipsPanel)] = typeof(UserMembershipsPanel),
            [nameof(UserProfilePanel)] = typeof(UserProfilePanel)
        };
        var type = map.TryGetValue(sectionKey, out var t) ? t : typeof(NotFoundPanel);
        return new RouteData(type, new Dictionary<string, object?>());
    }

    private static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return nameof(UserMembershipsPanel);
        var v = value.Trim();
        if (v.StartsWith("/")) v = v[1..];
        return v;
    }
}
