using BitUiNavigation.Client.Pages.ModalHost.Abstract;
using BitUiNavigation.Client.Pages.ModalHost.Helpers;
using BitUiNavigation.Client.Pages.ModalHost.Providers;
using BitUiNavigation.Client.Pages.UserProfile.Memberships;
using BitUiNavigation.Client.Pages.UserProfile.Profile;
using BitUiNavigation.Client.Pages.UserProfile.Sometimes;
using ModalHostState = BitUiNavigation.Client.Pages.ModalHost.State.ModalHostState;

namespace BitUiNavigation.Client.Pages.UserProfile.Provider;
public sealed class UserModalProvider : ModalProviderBase, IModalSave, IModalReset//, ISupportsAutoSave
{
    private readonly IValidator<UserProviderAggregate> _providerValidator;

    [Parameter, SupplyParameterFromQuery] public Guid AccountId { get; set; }
    [Parameter, SupplyParameterFromQuery] public Guid LocationId { get; set; }

    public override string ProviderName => "User";
    public override string DefaultPanel => nameof(UserMembershipsPanel);
    private UserModalState UserModalState => Store.GetState<UserModalState>();
    private ModalHostState ModalHostState => Store.GetState<ModalHostState>();

    /// <summary>
    /// If the provider does not support auto-save change this method to reflect that.
    /// This supersedes (overwrites) the entity modal state auto-save support result.
    /// If the provider does not support auto-save, return a result indicating that.
    /// Suggested text: "Auto save is not supported for {ProviderName}."
    /// </summary>
    public override AutoSaveSupportResult AutoSaveSupportResult => UserModalState.AutoSaveSupportResult;

    public bool CanSave => UserModalState.CanSave;
    public bool CanReset => UserModalState.CanReset;
    public bool IsResetting => UserModalState.IsResetting;
    public bool IsInitializing => UserModalState.IsInitializing;
    public bool IsSaving =>  UserModalState.IsSaving;

    public bool HasChanged => UserModalState.HasChanged;
    public override string InstanceName => string.IsNullOrWhiteSpace(UserModalState?.InstanceName)
        ? ProviderName
        : $"{UserModalState.InstanceName}";

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

        if (UserModalState.IsToggled)
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
        await UserModalState.SetInitializingBusy(true, ct);
        UserModalState.Initialize();
        await Task.CompletedTask;
    }
    public override async Task OnModalOpenedAsync(CancellationToken ct)
    {
        await UserModalState.SetInitializingBusy(true, ct);
        await UserModalState.InitializeData(AccountId, LocationId, ct);
        await ModalHostState.SetTitle(UserModalState.InstanceName, ct);
        await UserModalState.SetInitializingBusy(false, ct);
    }

    public async Task ResetAsync(CancellationToken ct) => await UserModalState.DiscardChanges();

    public override async Task<(bool, string, IReadOnlyList<string>)> ValidateProviderAsync(CancellationToken ct)
    {
        // 1) Build the aggregate snapshot from your current state
        var agg = new UserProviderAggregate(
            AccountId: AccountId,
            LocationId: LocationId
        );

        // 2) Run FluentValidation
        var result = await _providerValidator.ValidateAsync(agg, ct);

        // 3) Return (bool, messages)
        if (result is null || result.IsValid) 
            return (true, string.Empty, Array.Empty<string>());

        var generalMessages = result.Errors
            .Where(f => f.PropertyName == string.Empty)
            .Select(f => f.ErrorMessage)
            .ToList();

        var generalMessage = generalMessages.Any()
            ? string.Join(" ", generalMessages)
            : "There are validation errors.";

        var messages = result.Errors
           .Where(e => e.PropertyName != string.Empty)
           .Select(e => e.ErrorMessage)
           .Where(m => !string.IsNullOrWhiteSpace(m))
           .ToArray();

        return (false, generalMessage, messages ?? []);
    }

    public async Task SaveAsync(CancellationToken ct)
    {
        await UserModalState.SetIsSaving(true, ct);
        await UserModalState.SaveUser(ct);
        await UserModalState.SetIsSaving(false, ct);
    }

    public async ValueTask DisposeAsync()
    {
        await UserModalState.Clear(CancellationToken.None);
    }

    public override async Task ClearState(CancellationToken ct)
    {
        await UserModalState.Clear(ct);
    }

    public override bool HasUnsavedChanges => UserModalState.HasChanged;

}

