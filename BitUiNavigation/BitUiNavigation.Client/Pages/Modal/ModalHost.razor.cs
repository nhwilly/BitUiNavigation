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
        private bool _needsSessionInit = false;
        private bool MissingPanelValidityBlocksClose => false; // flip to true if you want stricter policy
        private List<string>? _providerValidationMessages = [];
        private ModalHostState _state => GetState<ModalHostState>();

        private ModalContext ModalContext => new()
        {
            ProviderKey = _modalProvider?.ProviderName ?? "UnknownProvider",
            PanelName = _panelName 
        };

        private bool _modalHostIsInitializing = true;
        // NEW: Blocking dialog local state
        private bool _isDialogOpen;
        private string _dialogTitle = string.Empty;
        private string _dialogBody = string.Empty;
        private string _dialogPrimaryText = "OK";
        private string _dialogSecondaryText = "Cancel";
        private bool _showDialogSecondary;
        private EventCallback _onDialogPrimary;
        private EventCallback _onDialogSecondary;

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
                await _state.InitializeModal();
                _preOpenUrl = RemoveModalQueryParameters(fullUri);
                await _modalProvider.OnModalOpeningAsync(CancellationToken);
                await _modalProvider.BuildNavSections(NavManager, CancellationToken);
                _needsSessionInit = true;
                Logger.LogDebug("Changing modal to '{Modal}' '{Panel}'", _modalProvider.ProviderName, _panelName);
            }

            if (requestStateHasChanged)
                StateHasChanged();
        }


        private async Task OnSaveClicked()
        {
            if (_modalProvider is not IModalSave modalSave) return;

            var (isValid, _) = await _modalProvider.ValidateProviderAsync(CancellationToken);
            if (!isValid)
            {
                ShowInfoDialog(
                    title: "Please fix validation issues",
                    body: "Your changes contain validation problems. Correct them and try again.",
                    primaryText: "OK"
                );
                return;
            }

            await modalSave.SaveAsync(CancellationToken);
            // if (!ok)
            // {
            //     ShowInfoDialog(
            //         title: "Unable to save",
            //         body: "There was a problem saving your changes. Please try again.",
            //         primaryText: "OK"
            //     );
            // }
            // No header projection to update here; buttons hide automatically when IsDirty flips in provider state.
        }

        private async Task OnResetClicked()
        {
            if (_modalProvider is not IModalReset modalReset) return;
            await modalReset.ResetAsync(CancellationToken);
        }

        private async Task DiscardAndCloseAsync()
        {
            // if (_saveReset is not null)
            //     await _saveReset.ResetAsync(CancellationToken);

            if (!string.IsNullOrEmpty(_preOpenUrl))
                NavManager.NavigateTo(_preOpenUrl!, replace: true);

            else if (_modalProvider is not null)
                NavManager.NavigateTo(NavManager.GetUriWithQueryParameter(_modalProvider.ProviderName, (string?)null), replace: true);

            _preOpenUrl = null;
            _modalProvider = null;
            StateHasChanged();
            await Task.CompletedTask;
        }


        private void ShowInfoDialog(string title, string body, string primaryText)
        {
            _dialogTitle = title;
            _dialogBody = body;
            _dialogPrimaryText = primaryText;
            _dialogSecondaryText = string.Empty;
            _showDialogSecondary = false;
            _onDialogPrimary = EventCallback.Factory.Create(this, () => { _isDialogOpen = false; StateHasChanged(); });
            _onDialogSecondary = default;
            _isDialogOpen = true;
            StateHasChanged();
        }

        private void ShowConfirmDialog(string title, string body, string primaryText, string secondaryText, Func<Task> onPrimary, Action onSecondary)
        {
            _dialogTitle = title;
            _dialogBody = body;
            _dialogPrimaryText = primaryText;
            _dialogSecondaryText = secondaryText;
            _showDialogSecondary = true;

            _onDialogPrimary = EventCallback.Factory.Create(this, async () =>
            {
                _isDialogOpen = false;
                await onPrimary();
            });

            _onDialogSecondary = EventCallback.Factory.Create(this, () =>
            {
                _isDialogOpen = false;
                onSecondary();
                StateHasChanged();
            });

            _isDialogOpen = true;
            StateHasChanged();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            Logger.LogDebug("OnAfterRender: Modal='{Modal}', Panel='{Panel}'", Modal, Panel);

            // we have a new modal session that just opened, so we call the provider
            if (_needsSessionInit && _modalProvider is not null)
            {
                _modalHostIsInitializing = false;
                await ReadFromUri(NavManager.Uri, requestStateHasChanged: true);
                _needsSessionInit = false;
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
            var areValid = _state.ArePanelsValid(providerKey, expectedPanelKeys, MissingPanelValidityBlocksClose);
            return await Task.FromResult(areValid);
        }
        private async Task<bool> IsProviderValid()
        {
            if (_modalProvider is null) return true;

            var (isValid, messages) = await _modalProvider.ValidateProviderAsync(CancellationToken);
            if (!isValid)
            {
                _providerValidationMessages = messages?.ToList() ?? [];
            }
            return isValid;
        }

        private async Task TryCloseAsync()
        {
            if (_modalProvider is null)
            {
                Logger.LogDebug("ModalProvider is null - closing.");
                CloseModal();
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
            var providerIsValid = await IsProviderValid();

            if (!panelsAreValid || !providerIsValid)
            {
                Logger.LogDebug("Cannot close: PanelsValid={PanelsValid}, ProviderValid={ProviderValid}", panelsAreValid, providerIsValid);
                // TODO: set flag to display validation dialogue in UI
                // this will allow the model to become valid through corrections or reset.
                StateHasChanged();
                return;
            }

            // now we know it is valid.  are there any changes to save?
            if (!_modalProvider.HasUnsavedChanges)
            {
                Logger.LogDebug("No unsaved changes - closing.");
                CloseModal();
                return;
            }

            // now it's valid and there are changes.  what to do?

            // if we can't save, we have to close without saving -
            // probably an error condition, log it or toast it or both
            if (_modalProvider is not IModalSave modalSave)
            {
                Logger.LogError("ModalProvider '{Provider}' has unsaved changes but does not support saving.", _modalProvider.ProviderName);
                // TODO: toast at a minimum, then close...
                CloseModal();
                return;
            }

            // if we can save but not on navigation
            // we need to set flag to display a choice of discard/save/cancel
            if (_modalProvider is not ISupportsSaveOnClose)
            {
                Logger.LogDebug("ModalProvider '{Provider}' has unsaved changes but does not support SaveOnClose.", _modalProvider.ProviderName);
                // TODO set flag here...
                CloseModal();
                return;
            }

            Logger.LogDebug("ModalProvider '{Provider}' has unsaved changes and supports SaveOnClose - saving.", _modalProvider.ProviderName);
            // so at this point, we are valid, have changes and can save on navigation.
            await modalSave.SaveAsync(CancellationToken);
            CloseModal();

        }

        private void CloseModal()
        {
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

            _preOpenUrl = null;
            _modalProvider = null;
            _panelName = string.Empty;
            _panel = null;

        }

        public override void Dispose()
        {
            NavManager.LocationChanged -= HandleLocationChanged;
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

        static BitNavClassStyles NavStyles => new()
        {
            SelectedItemContainer = "nav-selected-item-container",
            ItemContainer = "nav-item-container",
            Item = "nav-item",
            SelectedItem = "nav-selected-item",
        };

    }
}