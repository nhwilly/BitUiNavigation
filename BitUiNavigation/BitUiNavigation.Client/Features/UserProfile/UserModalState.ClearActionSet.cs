namespace BitUiNavigation.Client.Features.UserProfile;
public sealed partial class UserModalState
{
    public static class ClearActionSet
    {
        public sealed class Action : IAction { }
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
                State.User = null;
                State.ProfileVm = new();
                State.MembershipsVm = new();
                State.SometimesViewModel = new();
                State.ProfileVmOriginal = new();
                State.MembershipsVmOriginal = new();
                State.SometimesViewModelOriginal = new();
                _logger.LogDebug("Cleared UserModalState.");
                await Task.CompletedTask;
            }
        }
    }
}
