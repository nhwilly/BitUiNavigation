namespace BitUiNavigation.Client.Services;

public sealed partial class ModalHostState : State<ModalHostState>
{
    public override void Initialize() { _validity?.Clear(); }
    private readonly Dictionary<string, Dictionary<string, PanelValidity>> _validity = [];
    public IReadOnlyDictionary<string, Dictionary<string, PanelValidity>> Validity => _validity;
    public record PanelValidity(bool IsValid, int ErrorCount);

    public List<NavSectionDetail> NavSections { get; private set; } = [];
    /// <summary>
    /// True if every expected panel for the provider is valid.
    /// If a panel hasn't published yet, it's treated as valid unless missingBlocks==true.
    /// </summary>
    public bool ArePanelsValid(string providerKey,
                               IEnumerable<string> expectedPanelKeys,
                               bool missingBlocks = false)
    {
        if (!Validity.TryGetValue(providerKey, out var perPanel))
            return !missingBlocks; // nothing published yet

        foreach (var key in expectedPanelKeys)
        {
            if (!perPanel.TryGetValue(key, out var pv))
            {
                if (missingBlocks) return false;
                continue;
            }
            if (!pv.IsValid) return false;
        }
        return true;
    }

    public bool ShowResult { get; private set; }
    public static class ShowResultModalActionSet
    {
        public sealed class Action : IAction
        {
            public bool Show { get; }
            public string Title { get; }
            public string Message { get; }
            public Action(bool show, string title, string message)
            {
                Show = show;
                Title = title;
                Message = message;
            }
        }
        public sealed class Handler : ActionHandler<Action>
        {
            private readonly ILogger<ModalHostState> _logger;
            private ModalHostState State => Store.GetState<ModalHostState>();
            public Handler(IStore store, ILogger<ModalHostState> logger) : base(store)
            {
                _logger = logger;
            }
            public override async Task Handle(Action action, CancellationToken cancellationToken)
            {
                _logger.LogDebug("SetShowResultModal Show={Show}", action.Show);
                State.ShowResult = action.Show;
                await Task.CompletedTask;
            }
        }
    }

    public static class SetNavSectionsActionSet
    {
        public sealed class Action : IAction
        {
            public List<NavSectionDetail> NavSections { get; }
            public Action(List<NavSectionDetail> navSections)
            {
                NavSections = navSections ?? [];
            }
        }
        public sealed class Handler : ActionHandler<Action>
        {
            private readonly ILogger<ModalHostState> _logger;
            private ModalHostState ModalHostState => Store.GetState<ModalHostState>();
            public Handler(IStore store, ILogger<ModalHostState> logger) : base(store)
            {
                _logger = logger;
            }
            public override async Task Handle(Action action, CancellationToken cancellationToken)
            {
                _logger.LogDebug("SetNavSections Count={Count}", action.NavSections.Count);
                //ModalHostState.NavSections.Clear();
                //ModalHostState.NavSections.AddRange(action.NavSections);
                ModalHostState.NavSections = action.NavSections;
                await Task.CompletedTask;
            }
        }
    }
    public string Title { get; private set; } = string.Empty;
    public static class SetTitleActionSet
    {
        public sealed class Action : IAction
        {
            public string Title { get; }
            public Action(string title)
            {
                Title = title;
            }
        }
        public sealed class Handler : ActionHandler<Action>
        {
            private readonly ILogger<ModalHostState> _logger;
            private ModalHostState ModalHostState => Store.GetState<ModalHostState>();
            public Handler(IStore store, ILogger<ModalHostState> logger) : base(store)
            {
                _logger = logger;
            }

            public override async Task Handle(Action action, CancellationToken cancellationToken)
            {
                _logger.LogDebug("SetTitle={Title}", action.Title);
                ModalHostState.Title = action.Title;
                await Task.CompletedTask;
            }
        }
    }

    //public static class RefreshNavItemsActionSet
    //{
    //    public sealed class Action : IAction
    //    {
    //        public Action() { }
    //    }
    //    public sealed class Handler : ActionHandler<Action>
    //    {
    //        private readonly ILogger<UserModalState> _logger;
    //        private ModalHostState ModalHostState => Store.GetState<ModalHostState>();
    //        public Handler(
    //            IStore store,
    //            ILogger<UserModalState> logger)
    //            : base(store)
    //        {
    //            _logger = logger;
    //        }
    //        public override async Task Handle(Action action, CancellationToken cancellationToken)
    //        {
    //            Notify
    //            await Task.CompletedTask;
    //        }
    //    }
    //}

    public static class SetValidityActionSet
    {
        public sealed class Action : IAction
        {
            public string ProviderKey { get; }
            public string PanelName { get; }
            public bool IsValid { get; }
            public int ErrorCount { get; }
            public Action(string providerKey, string panelName, bool isValid, int errorCount)
            {
                ProviderKey = providerKey ?? throw new ArgumentNullException(nameof(providerKey));
                PanelName = panelName ?? throw new ArgumentNullException(nameof(panelName));
                IsValid = isValid;
                ErrorCount = errorCount;
            }
        }
        public sealed class Handler : ActionHandler<Action>
        {
            private readonly ILogger<ModalHostState> _logger;
            private ModalHostState ModalHostState => Store.GetState<ModalHostState>();
            public Handler(IStore store, ILogger<ModalHostState> logger) : base(store)
            {
                _logger = logger;
            }
            public override async Task Handle(Action action, CancellationToken cancellationToken)
            {
                _logger.LogDebug("SetValidity Provider={ProviderKey} Panel={PanelName} IsValid={IsValid} ErrorCount={ErrorCount}",
                    action.ProviderKey, action.PanelName, action.IsValid, action.ErrorCount);
                if (!ModalHostState._validity.TryGetValue(action.ProviderKey, out var panelDict))
                {
                    panelDict = new Dictionary<string, PanelValidity>(StringComparer.OrdinalIgnoreCase);
                    ModalHostState._validity[action.ProviderKey] = panelDict;
                }
                panelDict[action.PanelName] = new PanelValidity(action.IsValid, action.ErrorCount);
                await Task.CompletedTask;
            }
        }
    }

}
