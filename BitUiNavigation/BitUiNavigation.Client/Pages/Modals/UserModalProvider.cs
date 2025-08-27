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
    private UserModalState State => Store.GetState<UserModalState>();
    private ModalHostState ModalHostState => Store.GetState<ModalHostState>();
    public UserModalProvider(
        IStore store,
        ILogger<UserModalProvider> logger)
            : base(store, logger) { }

    protected override Dictionary<string, Type> PanelMap { get; } = new(StringComparer.OrdinalIgnoreCase)
    {
        [nameof(UserMembershipsPanel)] = typeof(UserMembershipsPanel),
        [nameof(UserProfilePanel)] = typeof(UserProfilePanel)
    };
    private void DecorateWithValidationIndicators(List<BitNavItem> items)
    {
        // Quick local snapshot to avoid multiple property calls
        var lastKnown = State.LastKnownValidityByType;

        foreach (var item in items)
        {
            var key = item?.Key ?? "";
            if (item is null || string.IsNullOrWhiteSpace(key) || !PanelMap.TryGetValue(key, out var panelType))
                continue;

            if (lastKnown.TryGetValue(panelType, out var isValid) && isValid == false)
            {
                // Use a clear error/status icon; pick whatever Bit icon suits your design
                item.IconName = BitIconName.StatusErrorFull;

                // Accessibility / hinting (optional)
                item.Title = string.IsNullOrWhiteSpace(item.Title)
                    ? "Has validation errors"
                    : $"{item.Title} (has errors)";
                item.AriaLabel = $"{item.Text} has validation errors";
            }
            else
            {
                // Leave the normal icon alone for valid/unknown
                // (If you want unknown to show a warning, you can add a branch here.)
            }
        }
    }
    private void DecorateCustomNavItemsWithValidationIndicators(List<CustomNavItem> items)
    {
        // Quick local snapshot to avoid multiple property calls
        var lastKnown = State.LastKnownValidityByType;

        foreach (var item in items)
        {
            var key = item?.Key ?? "";
            if (item is null || string.IsNullOrWhiteSpace(key) || !PanelMap.TryGetValue(key, out var panelType))
                continue;

            if (lastKnown.TryGetValue(panelType, out var isValid) && isValid == false)
            {
                // Use a clear error/status icon; pick whatever Bit icon suits your design
                item.ValidationIconName = BitIconName.StatusErrorFull;

                // Accessibility / hinting (optional)
                item.Title = string.IsNullOrWhiteSpace(item.Title)
                    ? "Has validation errors"
                    : $"{item.Title} (has errors)";
                item.AriaLabel = $"{item.Text} has validation errors";
            }
            else
            {
                // Leave the normal icon alone for valid/unknown
                // (If you want unknown to show a warning, you can add a branch here.)
            }
        }
    }

    public override List<BitNavItem> BuildNavItems(NavigationManager nav)
    {
        var items = new List<BitNavItem>
        {
            new() { Key = nameof(UserMembershipsPanel), Text = "Memberships", IconName = BitIconName.UserEvent, Url = BuildPanelUrl(nav, nameof(UserMembershipsPanel)) },
            new() { Key = nameof(UserProfilePanel), Text = "Profile", IconName = BitIconName.Contact, Url = BuildPanelUrl(nav, nameof(UserProfilePanel)) }
        };
        DecorateWithValidationIndicators(items);
        return items;
    }

    public override List<CustomNavItem> BuildCustomNavItems(NavigationManager nav)
    {
        var items = new List<CustomNavItem>
        {
            new() { Key = nameof(UserMembershipsPanel), Text = "Memberships", IconName = BitIconName.UserEvent, Url = BuildPanelUrl(nav, nameof(UserMembershipsPanel)) },
            new() { Key = nameof(UserProfilePanel), Text = "Profile", IconName = BitIconName.Contact, Url = BuildPanelUrl(nav, nameof(UserProfilePanel)) }
        };
        DecorateCustomNavItemsWithValidationIndicators(items);
        return items;
    }

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

}
