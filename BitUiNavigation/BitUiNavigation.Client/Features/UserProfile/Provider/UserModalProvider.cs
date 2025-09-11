using FluentValidation.Results;

namespace BitUiNavigation.Client.Features.UserProfile.Provider;
public sealed class UserModalProvider : ModalProviderBase, IModalSave, IModalReset, ISupportsAutoSave
{
    private readonly IValidator<UserProviderAggregate> _providerValidator;

    [Parameter, SupplyParameterFromQuery] public Guid AccountId { get; set; }
    [Parameter, SupplyParameterFromQuery] public Guid LocationId { get; set; }

    public override string ProviderName => "User";
    public override string DefaultPanel => nameof(UserMembershipsPanel);
    private UserModalState UserModalState => Store.GetState<UserModalState>();

    public override AutoSaveSupportResult AutoSaveSupportResult => UserModalState.AutoSaveSupportResult;

    public bool CanSave => UserModalState.CanSave;
    public bool CanReset => UserModalState.CanReset;
    public bool IsResetting => UserModalState.IsResetting;
    public bool IsInitializing => UserModalState.IsLoading;
    public bool IsSaving => UserModalState.IsSaving;
    public bool IsBusy => IsInitializing || IsSaving || IsResetting;
    public bool HasChanged => UserModalState.HasChanged;
    public override string ProviderTitle => string.IsNullOrWhiteSpace(UserModalState?.InstanceName)
        ? ProviderName
        : $"{ProviderName}: {UserModalState.InstanceName}";

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
        var sections = new List<NavSectionDetail>
        {
            new()
            {
                Title = "Settings",
                IconName = BitIconName.Settings,
                CustomNavItems =
                [
                    new() { Key = nameof(UserMembershipsPanel), Text = "Memberships", IconName = BitIconName.UserEvent, Url = BuildPanelRelativeUrl(nav,  nameof(UserMembershipsPanel)) },
                    new() { Key = nameof(UserProfilePanel), Text = "Profile", IconName = BitIconName.Contact, Url = BuildPanelRelativeUrl(nav,  nameof(UserProfilePanel)) }
                ]
            }
        };

        if (UserModalState.ShouldShowSomeSpecialPanel)
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
            AddValidationIndicators(section.CustomNavItems);
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

    public override async Task<ValidationResult> ValidateProvider(CancellationToken ct)
    {
        var agg = new UserProviderAggregate(
            AccountId: AccountId,
            LocationId: LocationId
        );

        var result = await _providerValidator.ValidateAsync(agg, ct);
        return result;
    }

    public async Task SaveAsync(CancellationToken ct)
    {
        if (ct.IsCancellationRequested) return;

        try
        {
            await UserModalState.SetIsSaving(true, ct);
            await UserModalState.SaveUser(ct);
            await UserModalState.SetIsSaving(false, ct);
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
            await UserModalState.SetIsSaving(false, CancellationToken.None);
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

