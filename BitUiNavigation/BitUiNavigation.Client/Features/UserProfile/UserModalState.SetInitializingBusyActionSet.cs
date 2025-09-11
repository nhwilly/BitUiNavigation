namespace BitUiNavigation.Client.Features.UserProfile;
public sealed partial class UserModalState
{
    public static class SetInitializingBusyActionSet
    {
        public sealed class Action : IAction
        {
            public bool IsInitializing { get; private set; }
            public Action(bool isInitializing)
            {
                IsInitializing = isInitializing;
            }
        }

        public sealed class Handler : ActionHandler<Action>
        {
            private readonly ILogger<UserModalState> _logger;
            private UserModalState State => Store.GetState<UserModalState>();

            public Handler(
                IStore store,
                ILogger<UserModalState> logger)
                : base(store)
            {
                _logger = logger;
            }

            public override async Task Handle(Action action, CancellationToken cancellationToken)
            {
                State.IsLoading = action.IsInitializing;
                _logger.LogDebug("Set IsInitializing to {IsInitializing}", action.IsInitializing);
                await Task.CompletedTask;
            }
        }
    }
}
