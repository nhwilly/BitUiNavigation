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
    public override string ProviderName => "User";
    public override string DefaultPanel => nameof(UserMembershipsPanel);
    public override string Width => "900px";
    public override string Height => "640px";
    private UserEditSessionState State => Store.GetState<UserEditSessionState>();
    private ModalHostState ModalHostState => Store.GetState<ModalHostState>();
    public UserModalProvider(IStore store) : base(store) { }

    protected override Dictionary<string, Type> PanelMap { get; } = new(StringComparer.OrdinalIgnoreCase)
    {
        [nameof(UserMembershipsPanel)] = typeof(UserMembershipsPanel),
        [nameof(UserProfilePanel)] = typeof(UserProfilePanel)
    };

    public override List<BitNavItem> BuildNavItems(NavigationManager nav)
        =>
        [
            new() { Key = nameof(UserMembershipsPanel), Text = "Memberships", IconName = BitIconName.UserEvent, Url = BuildPanelUrl(nav, nameof(UserMembershipsPanel)) },
            new() { Key = nameof(UserProfilePanel),     Text = "Profile",     IconName = BitIconName.Contact,   Url = BuildPanelUrl(nav, nameof(UserProfilePanel)) }
        ];

    public override async Task OnModalOpeningAsync(CancellationToken ct)
    {
        await State.SetIsLoading(true, ct);
        await Task.CompletedTask;
    }
    public override async Task OnModalOpenedAsync(CancellationToken ct)
    {
        await State.BeginUserEditSession(AccountId, LocationId, ct);
        await ModalHostState.SetTitle(State.ProviderTitle, ct);
        await State.SetIsLoading(false, ct);
    }
    public override async Task<bool> CanCloseAsync(CancellationToken ct)
    {
        var lastKnown = PanelRegistry.LastKnownValidityByType;

        foreach (var kv in PanelMap) // kv.Value is the component Type
        {
            var panelType = kv.Value;

            if (lastKnown.TryGetValue(panelType, out var isValid))
            {
                if (!isValid) return false; // block close
            }
            else
            {
                // If you want to require that the user visits every panel before closing,
                // uncomment the next line:
                // return Task.FromResult(false);
            }
        }
        return await Task.FromResult(true);
    }
}
