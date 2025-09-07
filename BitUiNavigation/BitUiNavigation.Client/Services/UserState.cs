namespace BitUiNavigation.Client.Services
{
    public sealed partial class UserState : State<UserState>
    {
        public bool PrefersAutoSave { get; private set; } = false;
        public override void Initialize()
        {
            PrefersAutoSave = false;
        }
        public static class SetPrefersAutoSaveActionSet
        {
            public sealed class Action : IAction
            {
                public bool PrefersAutoSave { get; }
                public Action(bool prefersAutoSave)
                {
                    PrefersAutoSave = prefersAutoSave;
                }
            }
            public sealed class Handler : ActionHandler<Action>
            {
                private readonly ILogger<UserState> _logger;
                private UserState UserState => Store.GetState<UserState>();
                public Handler(IStore store, ILogger<UserState> logger) : base(store)
                {
                    _logger = logger;
                }
                public override async Task Handle(Action action, CancellationToken cancellationToken)
                {
                    _logger.LogDebug("Setting PrefersAutoSave to {PrefersAutoSave}", action.PrefersAutoSave);
                    await Task.Delay(2000, cancellationToken);
                    UserState.PrefersAutoSave = action.PrefersAutoSave;
                }
            }
        }

    }
}
