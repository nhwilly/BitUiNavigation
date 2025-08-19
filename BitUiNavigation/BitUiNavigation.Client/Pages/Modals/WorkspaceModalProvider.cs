using Bit.BlazorUI;
using BitUiNavigation.Client.Pages.UserProfile;
using BitUiNavigation.Client.Services;
using Microsoft.AspNetCore.Components;
using TimeWarp.State;

namespace BitUiNavigation.Client.Pages.Modals;

public sealed class WorkspaceModalProvider : TimeWarpStateComponent, IModalProvider
{
    public string ProviderName => "Workspace";
    public string DefaultPanel => nameof(WorkspaceDetailsPanel);
    public string Width => "900px";
    public string Height => "640px";

    public List<BitNavItem> BuildNavItems(NavigationManager nav, string queryKey)
    {
        string url(string section)
        {
            var currentPath = "/" + nav.ToBaseRelativePath(nav.Uri).Split('?')[0];
            var qs = System.Web.HttpUtility.ParseQueryString(new Uri(nav.Uri).Query);
            qs.Set(queryKey, Normalize(section, DefaultPanel));
            return $"{currentPath}?{qs}";
        }

        return
        [
            new() { Key = nameof(WorkspaceDetailsPanel), Text = "Workspace", Url = url(nameof(WorkspaceDetailsPanel)) },
            new() { Key = nameof(UserProfilePanel),     Text = "Details",     Url = url(nameof(UserProfilePanel)) }
        ];
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

    private static string Normalize(string? value, string defaultSection)
    {
        if (string.IsNullOrWhiteSpace(value)) return defaultSection;
        var v = value.Trim();
        if (v.StartsWith('/')) v = v[1..];
        return v;
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        // Register the modal provider with the state manager
        UserModalState userModalState = GetState<UserModalState>();
    }
    public Task OnModalOpenedAsync(CancellationToken ct)
    {
        UserModalState userModalState = GetState<UserModalState>();
        throw new NotImplementedException();
    }
    public Task OnModalOpeningAsync(CancellationToken ct)
    {
        UserModalState userModalState = GetState<UserModalState>();
        throw new NotImplementedException();
    }
}
