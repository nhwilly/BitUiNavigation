namespace BitUiNavigation.Client.Pages.ModalHost.State;

public sealed partial class ModalHostState
{
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
                ModalHostState.NavSections = action.NavSections;
                await Task.CompletedTask;
            }
        }
    }

}
