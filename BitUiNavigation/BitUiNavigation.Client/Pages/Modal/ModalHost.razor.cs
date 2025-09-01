using Bit.BlazorUI;
using BitUiNavigation.Client.Pages.Modal.Abstract;
using BitUiNavigation.Client.Pages.Modal.Providers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace BitUiNavigation.Client.Pages.Modal
{
    public partial class ModalHost
    {
        [Parameter, SupplyParameterFromQuery] public string? Modal { get; set; }
        [Parameter, SupplyParameterFromQuery] public string? Panel { get; set; }

        private IModalProvider? _activeModalProvider;
        private string _panelName = string.Empty;
        private string? _preOpenUrl;
        private IModalPanel? _panel;
        private bool _needsSessionInit = false;
        private bool MissingPanelValidityBlocksClose => false; // flip to true if you want stricter policy
        private List<string>? _providerValidationMessages = [];
        private ModalHostState _state => GetState<ModalHostState>();

        private ModalContext _ctx => new()
        {
            ProviderKey = _activeModalProvider?.ProviderName ?? "UnknownProvider",
            PanelName = _panelName // or nameof(UserProfilePanel) if you map it
        };

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
            var hasModalInUri = !string.IsNullOrWhiteSpace(Modal);
            if (!hasModalInUri)
            {
                _activeModalProvider = null;
                _panelName = string.Empty;
                _preOpenUrl = null;
                return;
            }

            var modalDoesNotExist = _activeModalProvider is null;

            var modalRequestedIsDifferent = !string.Equals(_activeModalProvider?.ProviderName, Modal, StringComparison.OrdinalIgnoreCase);
            var panelNameMatchesCurrent = string.Equals(_panelName, Panel, StringComparison.OrdinalIgnoreCase);
            _panelName = Panel ?? _activeModalProvider?.DefaultPanel ?? string.Empty;

            // are we new or are we navigating to a new modal?
            if (modalDoesNotExist || modalRequestedIsDifferent)
            {
                _activeModalProvider = ServiceProvider.GetRequiredKeyedService<IModalProvider>(Modal);
                _preOpenUrl = RemoveModalQueryParameters(fullUri);
                await _activeModalProvider.OnModalOpeningAsync(CancellationToken);
                await _activeModalProvider.BuildNavSections(NavManager, CancellationToken);
                _needsSessionInit = true;
                Logger.LogDebug("Changing modal to '{Modal}' '{Panel}'", _activeModalProvider.ProviderName, _panelName);
            }

            if (requestStateHasChanged)
                StateHasChanged();
        }


        private async Task OnSaveClicked()
        {
            if (_activeModalProvider is not IModalSave modalSave) return;

            var (isValid, _) = await _activeModalProvider.ValidateProviderAsync(CancellationToken);
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
            if (_activeModalProvider is not IModalReset modalReset) return;
            await modalReset.ResetAsync(CancellationToken);
        }

        private async Task DiscardAndCloseAsync()
        {
            // if (_saveReset is not null)
            //     await _saveReset.ResetAsync(CancellationToken);

            if (!string.IsNullOrEmpty(_preOpenUrl))
                NavManager.NavigateTo(_preOpenUrl!, replace: true);

            else if (_activeModalProvider is not null)
                NavManager.NavigateTo(NavManager.GetUriWithQueryParameter(_activeModalProvider.ProviderName, (string?)null), replace: true);

            _preOpenUrl = null;
            _activeModalProvider = null;
            _isDialogOpen = false;
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
            if (_needsSessionInit && _activeModalProvider is not null)
            {
                await ReadFromUri(NavManager.Uri, requestStateHasChanged: true);
                _needsSessionInit = false;
                // call provider *after* first render
                await _activeModalProvider.OnModalOpenedAsync(CancellationToken);
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        private bool CanCloseActiveProvider()
        {
            if (_activeModalProvider is null) return true;

            var providerKey = _activeModalProvider.ProviderName;

            // Normalize your keys the same way you publish them from ModalPanelBase / ModalContext
            var expectedPanelKeys = _activeModalProvider.ExpectedPanelKeys; // <-- now public

            return GetState<ModalHostState>()
                .ArePanelsValid(providerKey, expectedPanelKeys, MissingPanelValidityBlocksClose);
        }

        private void HandleLocationChanged(object? sender, LocationChangedEventArgs e)
        {
            // fire-and-forget wrapper that calls the async method
            _ = OnLocationChanged(sender, e);
        }

        //protected override async Task OnInitializedAsync()
        //{
        //    Logger.LogDebug("OnInitializedAsync: Modal='{Modal}', Panel='{Panel}'", Modal, Panel);
        //    // we have to have this here because this modal is always running and it needs 
        //    // to inspect the URL for to see if it should open a modal or not.
        //    NavManager.LocationChanged += HandleLocationChanged;
        //    //await ReadFromUri(NavManager.Uri, requestStateHasChanged: false);
        //}

        /// <summary>
        ///We have to have this here because this modal is always running and it needs 
        /// to inspect the URL for to see if it should open a modal or not.
        /// </summary>
        protected override void OnInitialized()
        {
            Logger.LogDebug("OnInitializedAsync: Modal='{Modal}', Panel='{Panel}'", Modal, Panel);
            NavManager.LocationChanged += HandleLocationChanged;
            // Process the initial URL so we can support deep linking.
            _ = ReadFromUri(NavManager.Uri, requestStateHasChanged: false);
        }

        /// <summary>
        /// When the location changes, we need to parse the URL to see if we need to open a modal
        /// or move to another panel within the modal.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task OnLocationChanged(object? sender, LocationChangedEventArgs e)
            => await ReadFromUri(e.Location, requestStateHasChanged: true);

        private async Task TryCloseAsync()
        {
            if (_activeModalProvider is not IModalSave modalSave) return;

            if (!CanCloseActiveProvider())
            {
                // show a toast/snack bar/banner as you like
                return;
            }
            // Optional provider-specific hook (non-blocking if you don't need it)
            if (_activeModalProvider is IBeforeCloseHook hook)
            {
                var ok = await hook.OnBeforeCloseAsync(CancellationToken);
                if (!ok) return;
            }









            // if (_activeModalProvider is not null)
            // {
            //     var (isValid, messages) = await _activeModalProvider.ValidateProviderAsync(CancellationToken);
            //     if (!isValid)
            //     {
            //         cache messages for UI and block close
            //         _providerValidationMessages = messages?.ToList() ?? [];
            //         StateHasChanged();
            //         return;
            //     }

            //     clear any prior provider errors
            //     _providerValidationMessages = null;
            // }

            // NEW: Save-or-discard flow
            if (modalSave.HasChanged)
            {
                if (_activeModalProvider is not ISupportsSaveOnNavigate modalSaveOnNavigate)
                {
                    var (valid, _) = await _activeModalProvider!.ValidateProviderAsync(CancellationToken);
                    if (valid)
                    {
                        await modalSave.SaveAsync(CancellationToken);
                        // if (!saved)
                        // {
                        //     ShowInfoDialog(
                        //         title: "Unable to save before closing",
                        //         body: "There was a problem saving your changes. Please try again.",
                        //         primaryText: "OK"
                        //     );
                        //     return; keep modal open
                        // }
                    }
                    else
                    {
                        ShowConfirmDialog(
                            title: "Discard changes and close?",
                            body: "You have unsaved changes with validation issues.",
                            primaryText: "Discard & Close",
                            secondaryText: "Cancel",
                            onPrimary: async () => { await DiscardAndCloseAsync(); },
                            onSecondary: () => { /* keep open */ }
                        );
                        return;
                    }
                }
                else
                {
                    ShowConfirmDialog(
                        title: "Discard changes and close?",
                        body: "You have unsaved changes.",
                        primaryText: "Discard & Close",
                        secondaryText: "Cancel",
                        onPrimary: async () => { await DiscardAndCloseAsync(); },
                        onSecondary: () => { /* keep open */ }
                    );
                    return;
                }
            }



            // proceed with existing close logic...
            if (!string.IsNullOrEmpty(_preOpenUrl))
            {
                NavManager.NavigateTo(_preOpenUrl!, replace: true);
            }
            // the preOpenUrl was empty, let's just deduce it from our current location.
            else if (_activeModalProvider is not null)
            {
                var stripped = RemoveModalQueryParameters(NavManager.Uri);
                NavManager.NavigateTo(stripped, replace: true);
            }

            _preOpenUrl = null;
            _activeModalProvider = null;
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
            if (_activeModalProvider is null) return string.Empty;
            return $@"
        .{ModalContainerClass} {{
            width:  {_activeModalProvider.Width};
            height: {_activeModalProvider.Height};
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