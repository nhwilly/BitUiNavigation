namespace BitUiNavigation.Client.Pages.ModalHost.State;

public sealed partial class ModalHostState
{
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

}
