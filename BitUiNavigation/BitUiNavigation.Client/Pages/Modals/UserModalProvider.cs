using Bit.BlazorUI;
using BitUiNavigation.Client.Pages.UserProfile;
using BitUiNavigation.Client.Services;
using FluentValidation;
using Microsoft.AspNetCore.Components;
using TimeWarp.State;

namespace BitUiNavigation.Client.Pages.Modals;
public sealed class UserModalProvider : ModalProviderBase, IModalSave, IModalReset
{
    private readonly IValidator<UserProviderAggregate> _providerValidator;

    [Parameter, SupplyParameterFromQuery] public Guid AccountId { get; set; }
    [Parameter, SupplyParameterFromQuery] public Guid LocationId { get; set; }

    public override string ProviderName => "User";
    public override string DefaultPanel => nameof(UserMembershipsPanel);
    private UserModalState UserState => Store.GetState<UserModalState>();
    private ModalHostState ModalState => Store.GetState<ModalHostState>();
    public override bool AutoSaveOnNavigate => true; // return user preference or override it.
    public bool CanSave => UserState.CanSave;
    public bool CanReset => UserState.CanReset;
    public bool IsResetting => UserState.IsResetting;
    public bool IsInitializing => UserState.IsLoading;
    public bool HasChanged => UserState.HasChanged;
    public bool ShowResultDialog => ModalState.ShowResult;

    public bool SaveOnCloseEnabled => UserState.SaveOnCloseEnabled;

    public UserModalProvider(
        IStore store,
        IValidator<UserProviderAggregate> providerValidator,
        ILogger<UserModalProvider> logger)
            : base(store, logger)
    {
        _providerValidator = providerValidator;
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

    public override async Task OnModalOpeningAsync(CancellationToken ct)
    {
        await UserState.SetIsLoading(true, ct);
        await Task.CompletedTask;
    }
    public override async Task OnModalOpenedAsync(CancellationToken ct)
    {
        await UserState.BeginUserEditSession(AccountId, LocationId, ct);
        await ModalState.SetTitle(UserState.ProviderTitle, ct);
        await UserState.SetIsLoading(false, ct);
    }

    public async Task ResetAsync(CancellationToken ct)
    {

        await Task.CompletedTask;
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

    public async Task SaveAsync(CancellationToken ct)
    {
        await UserState.SetIsLoading(true, ct);
        await UserState.SaveUser(ct);
        await UserState.SetIsLoading(false, ct);
        if (ShowResultDialog)
            await ModalState.ShowResultModal(true, "title", "message", ct);
    }
}

