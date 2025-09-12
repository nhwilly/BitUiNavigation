namespace BitUiNavigation.Client.ModalHost.State;

public sealed partial class ModalHostState
{
    public static class ClearValidityActionSet
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
                ModalHostState._validity.Clear();
                await Task.CompletedTask;
            }
        }
    }

}
