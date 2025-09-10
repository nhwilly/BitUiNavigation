using BitUiNavigation.Client.Pages.ModalHost.Navigation;
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
        try
        {
            await HostState.SetNavSections(sections, ct);
        }
        catch (OperationCanceledException) { _logger.LogDebug("SetNavSections cancelled."); }
        catch (ObjectDisposedException) { _logger.LogDebug("SetNavSections CTS disposed."); }
    }

    public override async Task OnModalOpeningAsync(CancellationToken ct)
    {
        try
        {
            await UserModalState.SetInitializingBusy(true, ct);
            UserModalState.Initialize();
            await Task.CompletedTask;
        }
        catch (OperationCanceledException) { _logger.LogDebug("OnModalOpeningAsync cancelled."); }
        catch (ObjectDisposedException) { _logger.LogDebug("OnModalOpeningAsync CTS disposed."); }
    }
    public override async Task OnModalOpenedAsync(CancellationToken ct)
    {
        try
        {
            await UserModalState.SetInitializingBusy(true, ct);
            await UserModalState.InitializeData(AccountId, LocationId, ct);
            await ModalHostState.SetTitle(UserModalState.InstanceName, ct);
            await UserModalState.SetInitializingBusy(false, ct);
        }
        catch (OperationCanceledException) { _logger.LogDebug("OnModalOpenedAsync cancelled."); }
        catch (ObjectDisposedException) { _logger.LogDebug("OnModalOpenedAsync CTS disposed."); }
    }

    public async Task ResetAsync(CancellationToken ct)
    {
        try
        {
            await UserModalState.DiscardChanges();
        }
        catch (OperationCanceledException) { _logger.LogDebug("DiscardChanges cancelled."); }
        catch (ObjectDisposedException) { _logger.LogDebug("DiscardChanges CTS disposed."); }
    }

    public override async Task<(bool, string, IReadOnlyList<string>)> ValidateProviderAsync(CancellationToken ct)
    {
        var agg = new UserProviderAggregate(
            AccountId: AccountId,
            LocationId: LocationId
        );

        var result = await _providerValidator.ValidateAsync(agg, ct);

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
        if (ct.IsCancellationRequested) return;

        try
        {
            await UserModalState.SetIsSaving(true, ct);
            await UserModalState.SaveUser(ct);
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("SaveAsync cancelled.");
            return;
        }
        catch (ObjectDisposedException)
        {
            _logger.LogDebug("SaveAsync CTS disposed.");
            return;
        }
        finally
        {
            try
            {
                if (!ct.IsCancellationRequested)
                {
                    await UserModalState.SetIsSaving(false, ct);
                }
            }
            catch (OperationCanceledException) { }
            catch (ObjectDisposedException) { }
        }
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

