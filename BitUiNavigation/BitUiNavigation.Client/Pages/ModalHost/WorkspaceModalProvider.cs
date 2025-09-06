using Bit.BlazorUI;
using BitUiNavigation.Client.Pages.UserProfile;
using BitUiNavigation.Client.Services;
using Microsoft.AspNetCore.Components;
using TimeWarp.State;

namespace BitUiNavigation.Client.Pages.Modals;

public sealed class WorkspaceModalProvider : ModalProviderBase
{
    public override string ProviderName => "Workspace";
    public override string DefaultPanel => nameof(WorkspaceDetailsPanel);
    public override string Width => "900px";
    public override string Height => "640px";
    public override string ProviderTitle => _providerTitle;
    private string _providerTitle = string.Empty;

    public override List<BitNavItem> BuildNavItems(NavigationManager nav)
    {
        return
        [
            new() { Key = nameof(WorkspaceDetailsPanel), Text = "Workspace", Url = url(nameof(WorkspaceDetailsPanel)) },
            new() { Key = nameof(UserProfilePanel),     Text = "Details",     Url = url(nameof(UserProfilePanel)) }
        ];
    }

    public override RouteData BuildRouteData(string sectionKey)
    {
        var map = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
        {
            [nameof(WorkspaceDetailsPanel)] = typeof(WorkspaceDetailsPanel),
            [nameof(UserProfilePanel)] = typeof(UserProfilePanel)
        };
        var type = map.TryGetValue(sectionKey, out var t) ? t : typeof(NotFoundPanel);
        return new RouteData(type, new Dictionary<string, object?>());
    }


    protected override void OnInitialized()
    {
        base.OnInitialized();
        // Register the modal provider with the state manager
        UserModalState userModalState = GetState<UserModalState>();
    }
    public override Task OnModalOpenedAsync(CancellationToken ct)
    {
        UserModalState userModalState = GetState<UserModalState>();
        throw new NotImplementedException();
    }
    public override Task OnModalOpeningAsync(CancellationToken ct)
    {
        UserModalState userModalState = GetState<UserModalState>();
        throw new NotImplementedException();
    }
}
