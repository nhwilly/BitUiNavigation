namespace BitUiNavigation.Client.Pages.ModalHost.State;

public sealed partial class ModalHostState
{
    public static class SetIsSavingActionSet
    {
        public sealed class Action : IAction
        {
            public bool IsSaving { get; }
            public Action(bool isSaving)
            {
                IsSaving = isSaving;
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
                _logger.LogDebug("SetIsSaving IsSaving={IsSaving}", action.IsSaving);
                State.IsBusy = action.IsSaving;
                await Task.CompletedTask;
            }
        }
    }

}
