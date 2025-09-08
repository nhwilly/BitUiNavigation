using BitUiNavigation.Client.Pages.ModalHost.Components;
using BitUiNavigation.Client.Pages.ModalHost.State;

namespace BitUiNavigation.Client.Pages.ModalHost
{
    public partial class ModalHost
    {
        [Parameter, SupplyParameterFromQuery] public string? Modal { get; set; }
        [Parameter, SupplyParameterFromQuery] public string? Panel { get; set; }

        private IModalProvider? _modalProvider;
        private string _panelName = string.Empty;
        private string? _preOpenUrl;
        private IModalPanel? _panel;
        private bool _providerNeedsInit = false;
        private bool MissingPanelValidityBlocksClose => false;
        private List<string>? _providerValidationMessages = [];
        private string _providerValidationGeneralMessage = string.Empty;
        private ModalHostState ModalHostState => GetState<ModalHostState>();
        private UserState UserState => GetState<UserState>();
        private ModalContext ModalContext => new()
        {
            ProviderKey = _modalProvider?.ProviderName ?? "UnknownProvider",
            PanelName = _panelName
        };
        private async Task SetAutoSave() => await UserState.SetPrefersAutoSave(true);

        public bool IsSaving => (_modalProvider as IModalSave)?.IsSaving ?? false;

        private bool _modalHostIsInitializing = true;
        private bool _modalBusy { get; set; } = false;

        // Component lifetime token (from TimeWarpStateComponent) cached once
        private CancellationToken _componentLifetimeToken;

        // Per-navigation CTS and a linked CTS (nav + component lifetime)
        private CancellationTokenSource? _navCts;
        private CancellationTokenSource? _linkedNavCts;

        // Use this everywhere for provider/state calls
        private CancellationToken NavigationCancellationToken
        {
            get
            {
                if (_linkedNavCts is null)
                {
                    _navCts ??= new CancellationTokenSource();
                    try
                    {
                        // Link nav + component lifetime
                        _linkedNavCts = CancellationTokenSource.CreateLinkedTokenSource(_navCts.Token, _componentLifetimeToken);
                    }
                    catch (ObjectDisposedException)
                    {
                        // Fallback to nav-only if something was already disposed
                        _linkedNavCts = CancellationTokenSource.CreateLinkedTokenSource(_navCts.Token);
                    }
                }
                return _linkedNavCts.Token;
            }
        }

        private void CancelNavigationToken()
        {
            // cancel and dispose linked first (it depends on _navCts)
            try { _linkedNavCts?.Cancel(); } catch { }
            _linkedNavCts?.Dispose();
            _linkedNavCts = null;

            try { _navCts?.Cancel(); } catch { }
            _navCts?.Dispose();
            _navCts = null;
        }

        private async Task ReadFromUri(string fullUri, bool requestStateHasChanged)
        {
            var hasModalQueryStringParam = !string.IsNullOrWhiteSpace(Modal);
            if (!hasModalQueryStringParam)
            {
                _modalProvider = null;
                _panelName = string.Empty;
                _preOpenUrl = null;
                return;
            }

            var providerIsNull = _modalProvider is null;
            var providerRequestedIsDifferent = !string.Equals(_modalProvider?.ProviderName, Modal, StringComparison.OrdinalIgnoreCase);
            var panelNameMatchesCurrent = string.Equals(_panelName, Panel, StringComparison.OrdinalIgnoreCase);
            _panelName = Panel ?? _modalProvider?.DefaultPanel ?? string.Empty;

            if (providerIsNull || providerRequestedIsDifferent)
            {
                _modalProvider = ServiceProvider.GetRequiredKeyedService<IModalProvider>(Modal);
                ModalHostState.Initialize();
                _preOpenUrl = RemoveModalQueryParameters(fullUri);
                try
                {
                    _ = NavigationCancellationToken; // ensure linked CTS exists for this cycle
                    await _modalProvider.OnModalOpeningAsync(NavigationCancellationToken);
                    await _modalProvider.BuildNavSections(NavManager, NavigationCancellationToken);
                }
                catch (OperationCanceledException)
                {
                    Logger.LogDebug("OnModalOpening/BuildNavSections cancelled.");
                    return;
                }
                _providerNeedsInit = true;
                Logger.LogDebug("Changing modal to '{Modal}' '{Panel}'", _modalProvider.ProviderName, _panelName);
            }

            if (requestStateHasChanged)
                StateHasChanged();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            Logger.LogDebug("OnAfterRender: Modal='{Modal}', Panel='{Panel}'", Modal, Panel);

            if (_providerNeedsInit && _modalProvider is not null)
            {
                _modalHostIsInitializing = false;
                await ReadFromUri(NavManager.Uri, requestStateHasChanged: true);
                _providerNeedsInit = false;
                try
                {
                    await _modalProvider.OnModalOpenedAsync(NavigationCancellationToken);
                }
                catch (OperationCanceledException)
                {
                    Logger.LogDebug("OnModalOpenedAsync cancelled.");
                    return;
                }
                StateHasChanged();
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        private async Task OnUnsavedChangesOnSave()
        {
            if (_modalProvider is IModalSave modalSave)
            {
                try { await modalSave.SaveAsync(NavigationCancellationToken); }
                catch (OperationCanceledException) { Logger.LogDebug("SaveAsync cancelled."); return; }
            }
            await ModalHostState.SetModalAlertType(ModalAlertType.None);
        }

        private async Task OnValidationFailureDismiss()
        {
            if (ModalHostState.ModalAlertType is ModalAlertType.Validation or ModalAlertType.InvalidAggregate)
            {
                await ModalHostState.SetModalAlertType(ModalAlertType.None);
            }
        }
        private async Task OnResetClicked()
        {
            if (_modalProvider is not IModalReset modalReset) return;
            await ModalHostState.SetModalAlertType(ModalAlertType.ResetWarning, "Are you sure you want to discard all changes? This cannot be undone.");
        }

        private async Task ClearResetWarning()
        {
            if (ModalHostState.ModalAlertType == ModalAlertType.ResetWarning)
                await ModalHostState.SetModalAlertType(ModalAlertType.None);
        }

        private async Task OnDiscardClicked()
        {
            if (_modalProvider is IModalReset modalReset)
            {
                try { await modalReset.ResetAsync(NavigationCancellationToken); }
                catch (OperationCanceledException) { Logger.LogDebug("ResetAsync cancelled."); return; }
            }
            await ModalHostState.SetModalAlertType(ModalAlertType.None);
        }

        private void HandleLocationChanged(object? sender, LocationChangedEventArgs e) => _ = OnLocationChanged(sender, e);

        protected override async Task OnInitializedAsync()
        {
            // Cache the component lifetime token once to use for linking
            _componentLifetimeToken = CancellationToken;

            Logger.LogDebug("OnInitialized: Modal='{Modal}', Panel='{Panel}'", Modal, Panel);
            NavManager.LocationChanged += HandleLocationChanged;
            await ReadFromUri(NavManager.Uri, requestStateHasChanged: false);
        }

        private async Task OnLocationChanged(object? sender, LocationChangedEventArgs e)
        {
            CancelNavigationToken(); // tear down prior linked/nav CTS
            await ReadFromUri(e.Location, requestStateHasChanged: true);
        }

        private async Task<bool> ArePanelsValid()
        {
            if (_modalProvider is null) return true;

            var providerKey = _modalProvider.ProviderName;
            var expectedPanelKeys = _modalProvider.ExpectedPanelKeys;
            var areValid = ModalHostState.ArePanelsValid(providerKey, expectedPanelKeys, MissingPanelValidityBlocksClose);
            if (areValid) await ClearValidationAlert();
            else await ModalHostState.SetModalAlertType(ModalAlertType.Validation);

            return await Task.FromResult(areValid);
        }
        private async Task ClearValidationAlert()
        {
            if (ModalHostState.ModalAlertType == ModalAlertType.Validation)
                await ModalHostState.SetModalAlertType(ModalAlertType.None);
        }
        private async Task ClearUnsavedChangesAlert()
        {
            if (ModalHostState.ModalAlertType == ModalAlertType.UnsavedChanges)
                await ModalHostState.SetModalAlertType(ModalAlertType.None);
        }
        private async Task ClearInvalidAggregateAlert()
        {
            if (ModalHostState.ModalAlertType == ModalAlertType.InvalidAggregate)
                await ModalHostState.SetModalAlertType(ModalAlertType.None);
        }
        private async Task<bool> IsProviderValid()
        {
            if (_modalProvider is null) return true;

            try
            {
                var (isValid, generalMessage, messages) = await _modalProvider.ValidateProviderAsync(NavigationCancellationToken);
                if (!isValid)
                {
                    _providerValidationMessages = messages?.ToList() ?? [];
                    _providerValidationGeneralMessage = generalMessage;
                    await ModalHostState.SetModalAlertType(ModalAlertType.InvalidAggregate);
                }
                else
                {
                    await ClearInvalidAggregateAlert();
                }
                return isValid;
            }
            catch (OperationCanceledException)
            {
                Logger.LogDebug("ValidateProviderAsync cancelled.");
                return false;
            }
        }

        private async Task TrySaveAsync()
        {
            if (_modalProvider is null) { Logger.LogDebug("ModalProvider is null - nothing to save..."); return; }

            if (_modalProvider is IBeforeSaveHook hook)
            {
                try
                {
                    var canSave = await hook.OnBeforeSaveAsync(NavigationCancellationToken);
                    Logger.LogDebug("BeforeSaveHook returned {CanSave}", canSave);
                    if (!canSave) return;
                }
                catch (OperationCanceledException) { Logger.LogDebug("OnBeforeSaveAsync cancelled."); return; }
            }

            var panelsAreValid = await ArePanelsValid();
            if (!panelsAreValid) { Logger.LogDebug("Cannot close: PanelsValid={PanelsValid}", panelsAreValid); return; }

            var providerIsValid = await IsProviderValid();
            if (!providerIsValid) { Logger.LogDebug("Cannot close: ProviderValid={ProviderValid}", providerIsValid); return; }

            if (!_modalProvider.HasUnsavedChanges) { Logger.LogDebug("No unsaved changes - not saving."); return; }

            if (_modalProvider is not IModalSave modalSave)
            {
                Logger.LogError("ModalProvider '{Provider}' has unsaved changes but does not support saving.", _modalProvider.ProviderName);
                await ModalHostState.SetModalAlertType(ModalAlertType.Error, $"{_modalProvider.ProviderName} has unsaved changes but does not support saving.");
                return;
            }

            Logger.LogDebug("ModalProvider '{Provider}' has unsaved changes and supports Save - saving.", _modalProvider.ProviderName);
            try { await modalSave.SaveAsync(NavigationCancellationToken); }
            catch (OperationCanceledException) { Logger.LogDebug("SaveAsync cancelled."); return; }
        }

        private async Task TryCloseAsync()
        {
            if (_modalProvider is null)
            {
                Logger.LogDebug("ModalProvider is null - closing.");
                await CloseModalHost();
                return;
            }

            if (_modalProvider is IBeforeCloseHook hook)
            {
                try
                {
                    var canClose = await hook.OnBeforeCloseAsync(NavigationCancellationToken);
                    Logger.LogDebug("BeforeCloseHook returned {CanClose}", canClose);
                    if (!canClose) return;
                }
                catch (OperationCanceledException) { Logger.LogDebug("OnBeforeCloseAsync cancelled."); return; }
            }

            var panelsAreValid = await ArePanelsValid();
            if (!panelsAreValid) { Logger.LogDebug("Cannot close: PanelsValid={PanelsValid}", panelsAreValid); return; }

            var providerIsValid = await IsProviderValid();
            if (!providerIsValid) { Logger.LogDebug("Cannot close: ProviderValid={ProviderValid}", providerIsValid); return; }

            if (!_modalProvider.HasUnsavedChanges)
            {
                Logger.LogDebug("No unsaved changes - closing.");
                await CloseModalHost();
                return;
            }

            if (_modalProvider is not IModalSave modalSave)
            {
                Logger.LogError("ModalProvider '{Provider}' has unsaved changes but does not support saving.", _modalProvider.ProviderName);
                await ModalHostState.SetModalAlertType(ModalAlertType.Error, $"{_modalProvider.ProviderName} has unsaved changes but does not support saving.");
                return;
            }

            if (!_modalProvider.AutoSaveSupportResult.IsSupported)
            {
                Logger.LogDebug("ModalProvider '{Provider}' has unsaved changes but does not support SaveOnClose.", _modalProvider.ProviderName);
                await ModalHostState.SetModalAlertType(ModalAlertType.UnsavedChanges, _modalProvider.AutoSaveSupportResult?.Message ?? "Auto save not available.");
                return;
            }

            if (!UserState.PrefersAutoSave)
            {
                Logger.LogDebug("ModalProvider '{Provider}' has unsaved changes but does User not prefer auto save.", _modalProvider.ProviderName);
                await ModalHostState.SetModalAlertType(ModalAlertType.UnsavedChanges, "Auto save not enabled.");
                return;
            }

            Logger.LogDebug("ModalProvider '{Provider}' has unsaved changes and supports SaveOnClose - saving.", _modalProvider.ProviderName);
            try { await modalSave.SaveAsync(NavigationCancellationToken); }
            catch (OperationCanceledException) { Logger.LogDebug("SaveOnClose cancelled."); return; }

            await CloseModalHost();
        }

        private bool ShowEnableAutoSaveButton => _modalProvider?.AutoSaveSupportResult.IsSupported ?? false && !UserState.PrefersAutoSave;

        private async Task CloseModalHost()
        {
            CancelNavigationToken();

            if (_modalProvider is not null)
            {
                await _modalProvider.ClearState(CancellationToken.None);
            }

            if (!string.IsNullOrEmpty(_preOpenUrl))
            {
                NavManager.NavigateTo(_preOpenUrl!, replace: true);
            }
            else if (_modalProvider is not null)
            {
                var stripped = RemoveModalQueryParameters(NavManager.Uri);
                NavManager.NavigateTo(stripped, replace: true);
            }

            ModalHostState.Initialize();
            _preOpenUrl = null;
            _modalProvider = null;
            _panelName = string.Empty;
            _panel = null;
        }

        public override void Dispose()
        {
            CancelNavigationToken();
            NavManager.LocationChanged -= HandleLocationChanged;
            GC.SuppressFinalize(this);
            base.Dispose();
        }

        private static readonly string ModalContainerClass = "modal-container";

        private string CreateContainerCss()
        {
            if (_modalProvider is null) return string.Empty;
            return $@"
        .{ModalContainerClass} {{
            width:  {_modalProvider.Width};
            height: {_modalProvider.Height};
            border-radius:20px;
        }}";
        }

        readonly BitModalClassStyles ClassStyles = new() { Content = ModalContainerClass };
        readonly BitMessageClassStyles EnableAutoSaveStyle = new() { Actions = "padding: .5rem;" };
        static BitNavClassStyles NavStyles => new()
        {
            SelectedItemContainer = "nav-selected-item-container",
            ItemContainer = "nav-item-container",
            Item = "nav-item",
            SelectedItem = "nav-selected-item",
        };
    }
}