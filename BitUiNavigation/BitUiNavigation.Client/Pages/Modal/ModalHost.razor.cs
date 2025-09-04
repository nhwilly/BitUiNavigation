using BitUiNavigation.Client.Pages.Modal.Abstract;
using BitUiNavigation.Client.Pages.Modal.Components;

namespace BitUiNavigation.Client.Pages.Modal
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
        private bool MissingPanelValidityBlocksClose => false; // flip to true if you want stricter policy
        private List<string>? _providerValidationMessages = [];
        private string _providerValidationGeneralMessage = string.Empty;
        private ModalHostState ModalHostState => GetState<ModalHostState>();
        private UserState UserState => GetState<UserState>();
        private ModalContext ModalContext => new()
        {
            ProviderKey = _modalProvider?.ProviderName ?? "UnknownProvider",
            PanelName = _panelName
        };
        private async Task SetAutoSave()
        {
            await UserState.SetPrefersAutoSave(true);
        }

        private bool _modalHostIsInitializing = true;
        private bool _modalBusy { get; set; } = false;
        /// <summary>
        /// Inspects the provided URI and determines if it matches any of the model providers
        /// that were registered in dependency injection.  Selects a panel from the route
        /// or selects a default panel if none exists.  Finally it fires the OnModalOpeningAsync
        /// to allow the modal provider to initialize it's values.
        /// </summary>
        /// <param name="fullUri"></param>
        /// <param name="requestStateHasChanged"></param>
        private async Task ReadFromUri(string fullUri, bool requestStateHasChanged)
        {
            // if we don't have a valid modal query parameter, we are not active
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

            // are we new or are we navigating to a new modal?
            if (providerIsNull || providerRequestedIsDifferent)
            {
                _modalProvider = ServiceProvider.GetRequiredKeyedService<IModalProvider>(Modal);
                ModalHostState.Initialize();
                _preOpenUrl = RemoveModalQueryParameters(fullUri);
                await _modalProvider.OnModalOpeningAsync(CancellationToken);
                await _modalProvider.BuildNavSections(NavManager, CancellationToken);
                _providerNeedsInit = true;
                Logger.LogDebug("Changing modal to '{Modal}' '{Panel}'", _modalProvider.ProviderName, _panelName);
            }

            if (requestStateHasChanged)
                StateHasChanged();
        }

        private async Task OnUnsavedChangesOnSave()
        {
            if (_modalProvider is IModalSave modalSave)
            {
                await modalSave.SaveAsync(CancellationToken);
            }
            await ModalHostState.SetModalAlertType(ModalAlertType.None);
        }

        private async Task OnUnsavedChangesOnDiscard()
        {
            await ModalHostState.SetModalAlertType(ModalAlertType.ResetWarning);
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
            await modalReset.ResetAsync(CancellationToken);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            Logger.LogDebug("OnAfterRender: Modal='{Modal}', Panel='{Panel}'", Modal, Panel);

            // we have a new modal session that just opened, so we call the provider
            if (_providerNeedsInit && _modalProvider is not null)
            {
                _modalHostIsInitializing = false;
                await ReadFromUri(NavManager.Uri, requestStateHasChanged: true);
                _providerNeedsInit = false;
                await _modalProvider.OnModalOpenedAsync(CancellationToken);
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        /// <summary>
        /// Fire-and-forget wrapper that calls the async method.  OnLocationChanged is async
        /// but the subscription to the event handler is not.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleLocationChanged(object? sender, LocationChangedEventArgs e) => _ = OnLocationChanged(sender, e);

        /// <summary>
        /// Remember that the modal component is always included in the main layout.  This allows us to show a modal from anywhere.
        /// That means each navigation anywhere in the app will be reviewed to see if it contains a modal.
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            Logger.LogDebug("OnInitialized: Modal='{Modal}', Panel='{Panel}'", Modal, Panel);
            NavManager.LocationChanged += HandleLocationChanged;
            // Process the initial URL so we can support deep linking.
            await ReadFromUri(NavManager.Uri, requestStateHasChanged: false);
        }

        /// <summary>
        /// When the location changes, we need to parse the URL to see if we need to open a modal
        /// or move to another panel within the modal.  We need to fire StateHasChanged to 
        /// update the UI.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task OnLocationChanged(object? sender, LocationChangedEventArgs e)
            => await ReadFromUri(e.Location, requestStateHasChanged: true);

        private async Task<bool> ArePanelsValid()
        {
            if (_modalProvider is null) return true;

            var providerKey = _modalProvider.ProviderName;
            var expectedPanelKeys = _modalProvider.ExpectedPanelKeys;
            var areValid = ModalHostState.ArePanelsValid(providerKey, expectedPanelKeys, MissingPanelValidityBlocksClose);
            if (areValid)
            {
                await ClearValidationAlert();
            }
            else
            {
                await ModalHostState.SetModalAlertType(ModalAlertType.Validation);
            }

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

            var (isValid, generalMesage, messages) = await _modalProvider.ValidateProviderAsync(CancellationToken);
            if (!isValid)
            {
                _providerValidationMessages = messages?.ToList() ?? [];
                _providerValidationGeneralMessage = generalMesage;
                await ModalHostState.SetModalAlertType(ModalAlertType.InvalidAggregate);
            }
            else
            {
                await ClearInvalidAggregateAlert();
            }
            return isValid;
        }

        private async Task TryCloseAsync()
        {
            if (_modalProvider is null)
            {
                Logger.LogDebug("ModalProvider is null - closing.");
                await CloseModalHost();
                return;
            }

            // Optional pre-close hook (blocking if you implement it)
            // This may mutate state, so we do it first.
            if (_modalProvider is IBeforeCloseHook hook)
            {
                var canClose = await hook.OnBeforeCloseAsync(CancellationToken);
                Logger.LogDebug("BeforeCloseHook returned {CanClose}", canClose);
                if (!canClose) return;
            }
            // TODO: Possibly set a flag to indicate they tried to close but were blocked by validation.
            // then always do a provider aggregate validation with each normal validation.  Or we may have to
            // check for each field change.

            var panelsAreValid = await ArePanelsValid();

            if (!panelsAreValid)
            {
                Logger.LogDebug("Cannot close: PanelsValid={PanelsValid}", panelsAreValid);
                return;
            }

            var providerIsValid = await IsProviderValid();

            if (!providerIsValid)
            {
                Logger.LogDebug("Cannot close: ProviderValid={ProviderValid}", providerIsValid);
                return;
            }

            // now we know it is valid.  are there any changes to save?
            if (!_modalProvider.HasUnsavedChanges)
            {
                Logger.LogDebug("No unsaved changes - closing.");
                await CloseModalHost();
                return;
            }

            // now it's valid and there are changes.  what to do?

            // if we can't save, we have to close without saving -
            // probably an error condition, log it or toast it or both
            if (_modalProvider is not IModalSave modalSave)
            {
                Logger.LogError("ModalProvider '{Provider}' has unsaved changes but does not support saving.", _modalProvider.ProviderName);
                await ModalHostState.SetModalAlertType(ModalAlertType.Error);
                return;
            }

            // if we can save but not on navigation
            // we need to set flag to display a choice of discard/save/cancel
            if (_modalProvider is not ISupportsAutoSave)
            {
                Logger.LogDebug("ModalProvider '{Provider}' has unsaved changes but does not support SaveOnClose.", _modalProvider.ProviderName);
                // message bar to save their changes or discard before closing.  both buttons visible at that point.
                await ModalHostState.SetModalAlertType(ModalAlertType.UnsavedChanges);
                return;
            }

            Logger.LogDebug("ModalProvider '{Provider}' has unsaved changes and supports SaveOnClose - saving.", _modalProvider.ProviderName);
            // so at this point, we are valid, have changes and can save on navigation.
            await modalSave.SaveAsync(CancellationToken);
            await CloseModalHost();

        }

        private async Task CloseModalHost()
        {
            if (_modalProvider is not null)
            {
                await _modalProvider.ClearState(CancellationToken);
            }
            if (!string.IsNullOrEmpty(_preOpenUrl))
            {
                NavManager.NavigateTo(_preOpenUrl!, replace: true);
            }
            // the preOpenUrl was empty, let's just deduce it from our current location.
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

        readonly BitModalClassStyles ClassStyles = new()
        {
            Content = ModalContainerClass,
        };

        readonly BitMessageClassStyles EnableAutoSaveStyle = new()
        {
            Actions = "padding: .5rem;"

        };
        static BitNavClassStyles NavStyles => new()
        {
            SelectedItemContainer = "nav-selected-item-container",
            ItemContainer = "nav-item-container",
            Item = "nav-item",
            SelectedItem = "nav-selected-item",
        };

    }
}