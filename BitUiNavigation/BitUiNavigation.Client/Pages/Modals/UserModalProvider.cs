using Bit.BlazorUI;
using BitUiNavigation.Client.Pages.UserProfile;
using BitUiNavigation.Client.Services;
using FluentValidation;
using Microsoft.AspNetCore.Components;
using TimeWarp.State;

namespace BitUiNavigation.Client.Pages.Modals;
public sealed class UserModalProvider : ModalProviderBase
{
    private readonly IValidator<UserProviderAggregate> _providerValidator;

    [Parameter, SupplyParameterFromQuery] public Guid AccountId { get; set; }
    [Parameter, SupplyParameterFromQuery] public Guid LocationId { get; set; }
    public override string ProviderName => "User";
    public override string DefaultPanel => nameof(UserMembershipsPanel);
    public override string Width => "900px";
    public override string Height => "640px";
    private UserModalState State => Store.GetState<UserModalState>();
    private ModalHostState ModalHostState => Store.GetState<ModalHostState>();

    // This is for testing an aggregate of cross-panel facts.

    public UserModalProvider(
        IStore store,
        IValidator<UserProviderAggregate> providerValidator,
        ILogger<UserModalProvider> logger)
            : base(store, logger)
    {
        _providerValidator = providerValidator;
    }
    public override async Task<(bool, IReadOnlyList<string>)> ValidateProviderAsync(CancellationToken ct)
    {
        // 1) Build the aggregate snapshot from your current state
        var agg = new UserProviderAggregate(
            AccountId: AccountId,
            LocationId: LocationId
        );

        // 2) Run FluentValidation
        var result = await _providerValidator.ValidateAsync(agg, ct);

        // 3) Return (bool, messages)
        if (result.IsValid) return (true, Array.Empty<string>());

        var messages = result.Errors
            .Select(e => e.ErrorMessage)
            .Where(m => !string.IsNullOrWhiteSpace(m))
            .ToArray();

        return (false, messages);
    }
    protected override Dictionary<string, Type> PanelMap { get; } = new(StringComparer.OrdinalIgnoreCase)
    {
        [nameof(UserMembershipsPanel)] = typeof(UserMembershipsPanel),
        [nameof(UserProfilePanel)] = typeof(UserProfilePanel)
    };

    public override List<NavSectionDetail> BuildCustomNavSections(NavigationManager nav)
    {
        var sections = new List<NavSectionDetail>();
        sections.Add(new NavSectionDetail()
        {
            Title = "Settings",
            IconName = BitIconName.Settings,
            CustomNavItems =
                [
                    new() { Key = nameof(UserMembershipsPanel), Text = "Memberships", IconName = BitIconName.UserEvent, Url = BuildPanelUrl(nav, nameof(UserMembershipsPanel)) },
                    new() { Key = nameof(UserProfilePanel), Text = "Profile", IconName = BitIconName.Contact, Url = BuildPanelUrl(nav, nameof(UserProfilePanel)) }
                ]
        });

        foreach (var section in sections)
        {
            DecorateCustomNavItemsWithValidationIndicators(section.CustomNavItems);
        }
        return sections;
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
public sealed record UserProviderAggregate(
Guid AccountId,
Guid LocationId
// add any other cross-panel facts you need here
);
