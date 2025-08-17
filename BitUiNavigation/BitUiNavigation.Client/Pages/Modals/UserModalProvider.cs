using Bit.BlazorUI;
using BitUiNavigation.Client.Pages.UserProfile;
using BitUiNavigation.Client.Services;
using Microsoft.AspNetCore.Components;

namespace BitUiNavigation.Client.Pages.Modals;
public sealed class UserModalProvider : ModalProviderBase
{
    public override string QueryKey => "User";
    public override string DefaultSection => nameof(UserMembershipsPanel);
    public override string Width => "900px";
    public override string Height => "640px";
    [Parameter, SupplyParameterFromQuery] public Guid AccountId { get; set; }
    [Parameter, SupplyParameterFromQuery] public Guid LocationId { get; set; }
    private UserModalState UserModalState => GetState<UserModalState>();

    protected override Dictionary<string, Type> SectionMap { get; } = new(StringComparer.OrdinalIgnoreCase)
    {
        [nameof(UserMembershipsPanel)] = typeof(UserMembershipsPanel),
        [nameof(UserProfilePanel)] = typeof(UserProfilePanel)
    };

    public override List<BitNavItem> BuildNavItems(NavigationManager nav, string queryKey)
    {
        return
        [
            new() { Key = nameof(UserMembershipsPanel), Text = "Memberships", IconName=BitIconName.UserEvent, Url = BuildSectionUrl(nav, nameof(UserMembershipsPanel)) },
            new() { Key = nameof(UserProfilePanel),     Text = "Profile",     IconName=BitIconName.Contact,    Url = BuildSectionUrl(nav, nameof(UserProfilePanel)) }
        ];
    }
}
