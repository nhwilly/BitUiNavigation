using Bit.BlazorUI;
using BitUiNavigation.Client.Pages.UserProfile;
using Microsoft.AspNetCore.Components;

namespace BitUiNavigation.Client.Pages.Modals;

public sealed class WorkspaceModalProvider : IModalProvider
{
    public string QueryKey => nameof(WorkspaceModalProvider);
    public string DefaultSection => nameof(WorkspaceDetailsPanel);
    public string Width => "900px";
    public string Height => "640px";

    public List<BitNavItem> BuildNavItems(NavigationManager nav, string queryKey)
    {
        string url(string section) =>
            nav.GetUriWithQueryParameter(queryKey, Normalize(section));

        return new()
        {
            new() { Key = nameof(WorkspaceDetailsPanel), Text = "Workspace", Url = url(nameof(WorkspaceDetailsPanel)) },
            new() { Key = nameof(UserProfilePanel),     Text = "Details",     Url = url(nameof(UserProfilePanel)) }
        };
    }

    public RouteData BuildRouteData(string sectionKey)
    {
        var map = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
        {
            [nameof(WorkspaceDetailsPanel)] = typeof(WorkspaceDetailsPanel),
            [nameof(UserProfilePanel)] = typeof(UserProfilePanel)
        };
        var type = map.TryGetValue(sectionKey, out var t) ? t : typeof(NotFoundPanel);
        return new RouteData(type, new Dictionary<string, object?>());
    }

    private static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return nameof(WorkspaceDetailsPanel);
        var v = value.Trim();
        if (v.StartsWith("/")) v = v[1..];
        return v;
    }
}
