using TimeWarp.State;
using TypeSupport.Extensions;

namespace BitUiNavigation.Client.Services;

public sealed partial class ModalHostState : State<ModalHostState>
{
    public override void Initialize() { _validity?.Clear(); }
    private readonly Dictionary<string, Dictionary<string, PanelValidity>> _validity = [];
    public IReadOnlyDictionary<string, Dictionary<string, PanelValidity>> Validity => _validity;
    public record PanelValidity(bool IsValid, int ErrorCount);
    public bool IsChanged { get; private set; }
    public bool IsSaving { get; private set; }
    public bool IsSaved { get; private set; }
    public bool IsFetching { get; private set; }
    public bool IsReady { get; private set; }
    public string Title { get; private set; } = string.Empty;
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
    public static class SetChangedActionSet
    {
        public sealed class Action : IAction
        {
            public bool IsChanged { get; }
            public Action(bool isChanged)
            {
                IsChanged = isChanged;
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
                _logger.LogDebug("SetChanged IsChanged={IsChanged}", action.IsChanged);
                ModalHostState.IsChanged = action.IsChanged;
                await Task.CompletedTask;
            }
        }
    }
    public static class SetFetchingActionSet
    {
        public sealed class Action : IAction
        {
            public bool IsFetching { get; }

            public Action(bool fetching)
            {
                IsFetching = fetching;
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
                _logger.LogDebug("SetFetching.. ");
                ModalHostState.IsSaved = false;
                ModalHostState.IsSaving = false;
                ModalHostState.IsChanged = false;
                ModalHostState.IsFetching = action.IsFetching;
                await Task.CompletedTask;
            }
        }
    }
    public static class SetModelReadyActionSet
    {
        public sealed class Action : IAction
        {
            public bool IsReady { get; }

            public Action(bool isReady)
            {
                IsReady = isReady;
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
                _logger.LogDebug("SetReady.. {IsReady}", action.IsReady);
                ModalHostState.IsSaved = false;
                ModalHostState.IsSaving = false;
                ModalHostState.IsChanged = false;
                ModalHostState.IsFetching = false;
                ModalHostState.IsReady = action.IsReady;

                await Task.CompletedTask;
            }
        }
    }
    public static class SetSavedActionSet
    {
        public sealed class Action : IAction
        {
            public Action() { }
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
                _logger.LogDebug("SetSaved IsSaved");
                ModalHostState.IsSaved = true;
                ModalHostState.IsSaving = false;
                await Task.CompletedTask;
            }
        }
    }
    public static class SetSavingActionSet
    {
        public sealed class Action : IAction
        {
            public Action() { }
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
                _logger.LogDebug("SetSaving IsSaving");
                ModalHostState.IsSaving = true;
                await Task.CompletedTask;
            }
        }
    }
}
