namespace BitUiNavigation.Client.ModalHost.State;

public sealed partial class ModalHostState
{
    public static class SetValidityActionSet
    {
        public sealed class Action : IAction
        {
            public string ProviderName { get; }
            public string PanelName { get; }
            public bool IsValid { get; }
            public int ErrorCount { get; }
            public Action(string providerName, string panelName, bool isValid, int errorCount)
            {
                ProviderName = providerName ?? throw new ArgumentNullException(nameof(providerName));
                PanelName = panelName ?? throw new ArgumentNullException(nameof(panelName));
                IsValid = isValid;
                ErrorCount = errorCount;
            }
        }
        public sealed class Handler : ActionHandler<Action>
        {
            private readonly ILogger<ModalHostState> _logger;
            private ModalHostState ModalHostState => Store.GetState<ModalHostState>();
            private IServiceProvider _serviceProvider;
            public Handler(IStore store, ILogger<ModalHostState> logger, IServiceProvider serviceProvider) : base(store)
            {
                _logger = logger;
                _serviceProvider = serviceProvider;
            }
            public override async Task Handle(Action action, CancellationToken cancellationToken)
            {
                _logger.LogDebug("SetValidity Provider={ProviderKey} Panel={PanelName} IsValid={IsValid} ErrorCount={ErrorCount}",
                    action.ProviderName, action.PanelName, action.IsValid, action.ErrorCount);

                IModalProvider? provider;
                // if providerKey not found, add it
                if (!ModalHostState._validity.TryGetValue(action.ProviderName, out var panelDict))
                {
                    panelDict = new Dictionary<string, PanelValidity>(StringComparer.OrdinalIgnoreCase);
                    ModalHostState._validity[action.ProviderName] = panelDict;
                    panelDict[action.PanelName] = new PanelValidity(action.IsValid, action.ErrorCount);
                }
                // we have a providerKey, see if panel exists
                else
                {
                    var panelExists = panelDict.TryGetValue(action.PanelName, out var existingPanel);
                    if (panelExists)
                    {
                        if (existingPanel?.IsValid == action.IsValid && existingPanel.ErrorCount == action.ErrorCount) // no need to redraw
                        {
                            return;
                        }
                        else // panel is already in the list, and doesn't match, update it and redecorate
                        {
                            panelDict[action.PanelName] = new PanelValidity(action.IsValid, action.ErrorCount);
                            provider = _serviceProvider.GetRequiredKeyedService<IModalProvider>(action.ProviderName);
                            await provider.AddValidationToSections(cancellationToken);
                            return;
                        }
                    }
                    // panel not found, add it
                    panelDict[action.PanelName] = new PanelValidity(action.IsValid, action.ErrorCount);
                    //provider = _serviceProvider.GetRequiredKeyedService<IModalProvider>(action.ProviderName);
                }
                await Task.CompletedTask;
            }
        }
    }

}
