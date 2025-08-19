using Bit.BlazorUI;
using BitUiNavigation.Client.Pages.UserProfile;
using BitUiNavigation.Client.Services;
using Microsoft.AspNetCore.Components;
using TimeWarp.State;

namespace BitUiNavigation.Client.Pages.Modals;
public sealed class UserModalProvider : ModalProviderBase
{
    [Parameter, SupplyParameterFromQuery] public Guid AccountId { get; set; }
    [Parameter, SupplyParameterFromQuery] public Guid LocationId { get; set; }
    public override string ProviderKey => "User";
    public override string DefaultPanel => nameof(UserMembershipsPanel);
    public override string Width => "900px";
    public override string Height => "640px";
    private UserEditSessionState State;
    public UserModalProvider(IStore store) : base(store)
    {
        State = store.GetState<UserEditSessionState>();
    }
    protected override Dictionary<string, Type> PanelMap { get; } = new(StringComparer.OrdinalIgnoreCase)
    {
        [nameof(UserMembershipsPanel)] = typeof(UserMembershipsPanel),
        [nameof(UserProfilePanel)] = typeof(UserProfilePanel)
    };

    public override List<BitNavItem> BuildNavItems(NavigationManager nav, string queryKey)
        => new()
        {
            new() { Key = nameof(UserMembershipsPanel), Text = "Memberships", IconName = BitIconName.UserEvent, Url = BuildPanelUrl(nav, nameof(UserMembershipsPanel)) },
            new() { Key = nameof(UserProfilePanel),     Text = "Profile",     IconName = BitIconName.Contact,   Url = BuildPanelUrl(nav, nameof(UserProfilePanel)) }
        };

    public override async Task OnModalOpeningAsync(CancellationToken ct)
    {
        await State.SetIsLoading(true, ct);
    }
    public override async Task OnModalOpenedAsync(CancellationToken ct)
    {
        await State.BeginUserEditSession(AccountId, LocationId, ct);
        await State.SetIsLoading(false, ct);
    }

}
