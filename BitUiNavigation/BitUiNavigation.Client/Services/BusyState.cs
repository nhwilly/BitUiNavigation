using TimeWarp.State;

namespace BitUiNavigation.Client.Services;

public sealed partial class BusyState : State<BusyState>
{
    public override void Initialize() { }
    public bool AppIsBusy { get; private set; }
    public bool ModalIsBusy { get; private set; }
    public bool PageIsBusy { get; private set; }
    public bool ComponentIsBusy { get; private set; }
    public static class SetAppIsBusyActionSet
    {
        public sealed class Action : IAction
        {
            public bool IsBusy { get; }
            public Action(bool isBusy)
            {
                IsBusy = isBusy;
            }
        }
        public sealed class Handler : ActionHandler<Action>
        {
            private readonly ILogger<BusyState> _logger;
            private BusyState BusyState => Store.GetState<BusyState>();
            public Handler(IStore store, ILogger<BusyState> logger) : base(store)
            {
                _logger = logger;
            }
            public override Task Handle(Action action, CancellationToken cancellationToken)
            {
                _logger.LogDebug("Setting AppIsBusy to {IsBusy}", action.IsBusy);
                BusyState.AppIsBusy = action.IsBusy;
                return Task.CompletedTask;
            }
        }
    }
    public static class SetModalIsBusyActionSet
    {
        public sealed class Action : IAction
        {
            public bool IsBusy { get; }
            public Action(bool isBusy)
            {
                IsBusy = isBusy;
            }
        }
        public sealed class Handler : ActionHandler<Action>
        {
            private readonly ILogger<BusyState> _logger;
            private BusyState BusyState => Store.GetState<BusyState>();
            public Handler(IStore store, ILogger<BusyState> logger) : base(store)
            {
                _logger = logger;
            }
            public override Task Handle(Action action, CancellationToken cancellationToken)
            {
                _logger.LogDebug("Setting ModalIsBusy to {IsBusy}", action.IsBusy);
                BusyState.ModalIsBusy = action.IsBusy;
                return Task.CompletedTask;
            }
        }
    }
    public sealed class SetPageIsBusyActionSet
    {
        public sealed class Action : IAction
        {
            public bool IsBusy { get; }
            public Action(bool isBusy)
            {
                IsBusy = isBusy;
            }
        }
        public sealed class Handler : ActionHandler<Action>
        {
            private readonly ILogger<BusyState> _logger;
            private BusyState BusyState => Store.GetState<BusyState>();
            public Handler(IStore store, ILogger<BusyState> logger) : base(store)
            {
                _logger = logger;
            }
            public override Task Handle(Action action, CancellationToken cancellationToken)
            {
                _logger.LogDebug("Setting PageIsBusy to {IsBusy}", action.IsBusy);
                BusyState.PageIsBusy = action.IsBusy;
                return Task.CompletedTask;
            }
        }
    }

    public sealed class SetComponentIsBusyActionSet
    {
        public sealed class Action : IAction
        {
            public bool IsBusy { get; }
            public Action(bool isBusy)
            {
                IsBusy = isBusy;
            }
        }
        public sealed class Handler : ActionHandler<Action>
        {
            private readonly ILogger<BusyState> _logger;
            private BusyState BusyState => Store.GetState<BusyState>();
            public Handler(IStore store, ILogger<BusyState> logger) : base(store)
            {
                _logger = logger;
            }
            public override Task Handle(Action action, CancellationToken cancellationToken)
            {
                _logger.LogDebug("Setting ComponentIsBusy to {IsBusy}", action.IsBusy);
                BusyState.ComponentIsBusy = action.IsBusy;
                return Task.CompletedTask;
            }
        }
    }
}
