using BitUiNavigation.Client.Pages.UserProfile.Memberships;
using BitUiNavigation.Client.Pages.UserProfile.Profile;
using BitUiNavigation.Client.Pages.UserProfile.Sometimes;

namespace BitUiNavigation.Client.Pages.UserProfile.Provider;
public sealed class UserModalProvider : ModalProviderBase, IModalSave, IModalReset, ISupportsSaveOnClose
{
    private readonly IValidator<UserProviderAggregate> _providerValidator;

    [Parameter, SupplyParameterFromQuery] public Guid AccountId { get; set; }
    [Parameter, SupplyParameterFromQuery] public Guid LocationId { get; set; }

    public override string ProviderName => "User";
    public override string DefaultPanel => nameof(UserMembershipsPanel);
    private UserModalState UserState => Store.GetState<UserModalState>();
    private ModalHostState ModalHostState => Store.GetState<ModalHostState>();
    public override bool AutoSaveOnNavigate => true; // return user preference or override it.
    public bool CanSave => UserState.CanSave;
    public bool CanReset => UserState.CanReset;
    public bool IsResetting => UserState.IsResetting;
    public bool IsInitializing => UserState.IsInitializing;
    public bool HasChanged => UserState.HasChanged;
    public bool ShowResultDialog => ModalHostState.ShowResult;
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
        [nameof(UserProfilePanel)] = typeof(UserProfilePanel),
        [nameof(SometimesPanel)] = typeof(SometimesPanel),
    };

    public override async Task BuildNavSections(NavigationManager nav, CancellationToken ct)
    {
        var sections = new List<NavSectionDetail>();
        sections.Add(new NavSectionDetail()
        {
            Title = "Settings",
            IconName = BitIconName.Settings,
            CustomNavItems =
                [
                    new() { Key = nameof(UserMembershipsPanel), Text = "Memberships", IconName = BitIconName.UserEvent, Url = BuildPanelRelativeUrl(nav,  nameof(UserMembershipsPanel)) },
                    new() { Key = nameof(UserProfilePanel), Text = "Profile", IconName = BitIconName.Contact, Url = BuildPanelRelativeUrl(nav,  nameof(UserProfilePanel)) }
                ]
        });

        if (UserState.IsToggled)
        {
            sections.Add(new NavSectionDetail()
            {
                Title = "Sometimes",
                IconName = BitIconName.Calendar,
                CustomNavItems =
                [
                    new() { Key = nameof(SometimesPanel), Text = "Sometimes", IconName = BitIconName.Calendar, Url = BuildPanelRelativeUrl(nav,  nameof(SometimesPanel)) }
                ]
            });
        }
        foreach (var section in sections)
        {
            DecorateCustomNavItemsWithValidationIndicators(section.CustomNavItems);
        }
        await HostState.SetNavSections(sections, ct);
    }

    public override async Task OnModalOpeningAsync(CancellationToken ct)
    {
        // await UserState.SetIsLoading(true, ct);
        await UserState.Initialize(AccountId, LocationId, ct);
        await Task.CompletedTask;
        // await UserState.SetIsLoading(false, ct);
    }
    public override async Task OnModalOpenedAsync(CancellationToken ct)
    {
        await ModalHostState.SetTitle(UserState.ProviderTitle, ct);
        // await UserState.SetIsLoading(false, ct);
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
        // await UserState.SetIsLoading(true, ct);
        await UserState.SaveUser(ct);
        // await UserState.SetIsLoading(false, ct);
        if (ShowResultDialog)
            await ModalHostState.ShowResultModal(true, "title", "message", ct);
    }

    public async ValueTask DisposeAsync()
    {
        await UserState.Clear(CancellationToken.None);
    }

    public override async Task ClearState(CancellationToken ct)
    {
        await UserState.Clear(ct);
    }

    public override bool HasUnsavedChanges=>UserState.HasChanged;
}

