namespace BitUiNavigation.Client.ModalHost
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
        private bool ValidateMissingPanels => false;
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
        private BitVisibility ResetVisible => (_modalProvider as IModalReset)?.CanReset ?? false ? BitVisibility.Visible : BitVisibility.Collapsed;
        private BitVisibility SaveVisible => (_modalProvider as IModalSave)?.CanSave ?? false ? BitVisibility.Visible : BitVisibility.Collapsed;

        //private bool _modalHostIsInitializing = true;
        //private bool _modalBusy { get; set; } = false;

        // Component lifetime token (from TimeWarpStateComponent) cached once
        private CancellationToken _componentLifetimeToken;

        // Per-navigation CTS and a linked CTS (nav + component lifetime)
        private CancellationTokenSource? _navCts;
        private CancellationTokenSource? _linkedNavCts;

        protected override async Task OnInitializedAsync()
        {
            // Cache the component lifetime token once to use for linking
            _componentLifetimeToken = CancellationToken;

            Logger.LogDebug("OnInitialized: Modal='{Modal}', Panel='{Panel}'", Modal, Panel);
            NavManager.LocationChanged += HandleLocationChanged;
            await ReadFromUri(NavManager.Uri, requestStateHasChanged: false);
        }

        private void HandleLocationChanged(object? sender, LocationChangedEventArgs e) => _ = OnLocationChanged(sender, e);

        private async Task OnLocationChanged(object? sender, LocationChangedEventArgs e)
        {
            CancelNavigationToken(); // tear down prior linked/nav CTS
            await ReadFromUri(e.Location, requestStateHasChanged: true);
        }

        // Use this everywhere for provider/state calls
        private CancellationToken LinkedCancellationToken
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
                    _ = LinkedCancellationToken; // ensure linked CTS exists for this cycle
                    await _modalProvider.OnModalOpeningAsync(LinkedCancellationToken);
                    await _modalProvider.BuildNavSections(NavManager, LinkedCancellationToken);
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
                //_modalHostIsInitializing = false;
                await ReadFromUri(NavManager.Uri, requestStateHasChanged: true);
                _providerNeedsInit = false;
                try
                {
                    await _modalProvider.OnModalOpenedAsync(LinkedCancellationToken);
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
                try { await modalReset.ResetAsync(LinkedCancellationToken);
                await _modalProvider.AddValidationIndicators}
                catch (OperationCanceledException) { Logger.LogDebug("ResetAsync cancelled."); return; }
            }
            await ModalHostState.SetModalAlertType(ModalAlertType.None);
        }

        private async Task<bool> ArePanelsValid()
        {
            if (_modalProvider is null) return true;

            var providerKey = _modalProvider.ProviderName;
            var expectedPanelKeys = _modalProvider.ExpectedPanelKeys;
            var areValid = ModalHostState.ArePanelsValid(providerKey, expectedPanelKeys, ValidateMissingPanels);
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
                var result = await _modalProvider.ValidateProvider(LinkedCancellationToken);

                if (!result.IsValid)
                {
                    // validation errors not attached to a single property are given
                    // empty property names.  This allows for general error messages.
                    // These empty names are set in the FluentValidation abstract Validator<T>
                    var generalMessages = result.Errors
                    .Where(f => f.PropertyName == string.Empty)
                    .Select(f => f.ErrorMessage)
                    .ToList();

                    var _providerValidationGeneralMessage = generalMessages.Any()
                        ? string.Join(" ", generalMessages)
                        : "There are validation errors.";

                    var _providerValidationMessages = result.Errors
                       .Where(e => e.PropertyName != string.Empty)
                       .Select(e => e.ErrorMessage)
                       .Where(m => !string.IsNullOrWhiteSpace(m))
                       .ToArray();

                    await ModalHostState.SetModalAlertType(ModalAlertType.InvalidAggregate);
                }
                else
                {
                    await ClearInvalidAggregateAlert();
                }
                return result.IsValid;
            }
            catch (OperationCanceledException)
            {
                Logger.LogDebug("IsProviderValid cancelled.");
                return false;
            }
        }

        private async Task<bool> TrySaveAsync(bool closeModalInProgress = false)
        {
            try
            {
                await ModalHostState.SetIsSaving(true, LinkedCancellationToken);
                if (_modalProvider is null) { Logger.LogDebug("ModalProvider is null - nothing to save..."); return true; }

                if (_modalProvider is IBeforeSaveHook hook)
                {
                    try
                    {
                        var canSave = await hook.OnBeforeSaveAsync(LinkedCancellationToken);
                        Logger.LogDebug("BeforeSaveHook returned {CanSave}", canSave);
                        if (!canSave) return false;
                    }
                    catch (OperationCanceledException) { Logger.LogDebug("OnBeforeSaveAsync cancelled."); return false; }
                }

                var panelsAreValid = await ArePanelsValid();
                if (!panelsAreValid) { Logger.LogDebug("Cannot save: PanelsValid={PanelsValid}", panelsAreValid); return false; }

                var providerIsValid = await IsProviderValid();
                if (!providerIsValid) { Logger.LogDebug("Cannot save: ProviderValid={ProviderValid}", providerIsValid); return false; }

                if (!_modalProvider.HasUnsavedChanges) { Logger.LogDebug("No unsaved changes - not saving."); return true; }

                if (_modalProvider is not IModalSave modalSave)
                {
                    Logger.LogError("ModalProvider '{Provider}' has unsaved changes but does not support saving.", _modalProvider.ProviderName);
                    await ModalHostState.SetModalAlertType(ModalAlertType.Error, $"{_modalProvider.ProviderName} has unsaved changes but does not support saving.");
                    return false;
                }

                if (closeModalInProgress && !UserState.PrefersAutoSave)
                {
                    Logger.LogDebug("ModalProvider '{Provider}' has unsaved changes but does User not prefer auto save.", _modalProvider.ProviderName);
                    await ModalHostState.SetModalAlertType(ModalAlertType.UnsavedChanges, "Auto save not enabled.");
                    return false;
                }

                Logger.LogDebug("ModalProvider '{Provider}' has unsaved changes and supports Save - saving.", _modalProvider.ProviderName);
                try { await modalSave.SaveAsync(LinkedCancellationToken); }
                catch (OperationCanceledException) { Logger.LogDebug("SaveAsync cancelled."); return false; }
                return true;
            }
            catch (Exception)
            {
            }
            finally
            {
                await ModalHostState.SetIsSaving(false, LinkedCancellationToken);
            }
            return false;
        }

        private async Task TryCloseAsync()
        {
            var readyToClose = await TrySaveAsync(closeModalInProgress: true);
            if (!readyToClose) return;
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
    }
}