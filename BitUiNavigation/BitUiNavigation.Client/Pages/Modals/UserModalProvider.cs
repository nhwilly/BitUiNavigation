using Bit.BlazorUI;
using BitUiNavigation.Client.Pages.UserProfile;
using BitUiNavigation.Client.Services;
using Microsoft.AspNetCore.Components;
using TimeWarp.State;

public sealed class UserModalProvider : ModalProviderBase
{
    [Parameter, SupplyParameterFromQuery] public Guid AccountId { get; set; }
    [Parameter, SupplyParameterFromQuery] public Guid LocationId { get; set; }
    public override string QueryKey => "User";
    public override string DefaultSection => nameof(UserMembershipsPanel);
    public override string Width => "900px";
    public override string Height => "640px";
    public UserModalProvider(IStore store) : base(store) { }
    protected override Dictionary<string, Type> SectionMap { get; } = new(StringComparer.OrdinalIgnoreCase)
    {
        [nameof(UserMembershipsPanel)] = typeof(UserMembershipsPanel),
        [nameof(UserProfilePanel)] = typeof(UserProfilePanel)
    };

    //private UserEditSessionState UserEditSessionState => GetState<UserEditSessionState>();
    public override List<BitNavItem> BuildNavItems(NavigationManager nav, string queryKey)
        => new()
        {
            new() { Key = nameof(UserMembershipsPanel), Text = "Memberships", IconName = BitIconName.UserEvent, Url = BuildSectionUrl(nav, nameof(UserMembershipsPanel)) },
            new() { Key = nameof(UserProfilePanel),     Text = "Profile",     IconName = BitIconName.Contact,   Url = BuildSectionUrl(nav, nameof(UserProfilePanel)) }
        };

    public override Task OnModalOpenedAsync(CancellationToken ct)
    {
        var state = Store.GetState<UserEditSessionState>();

        // set loading flag
        _ = state.SetIsLoading(true, ct);

        // fire-and-forget load (this will run after the current sync context returns
        Task.Run(async () =>
        {
            await state.BeginUserEditSession(AccountId, LocationId, ct);
            await state.SetIsLoading(false, ct);
        }, ct);

        return Task.CompletedTask;
    }

}
