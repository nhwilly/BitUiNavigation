namespace BitUiNavigation.Client.Features.UserProfile;
public sealed partial class UserModalState
{
    public static class DiscardChangesActionSet
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
                _logger.LogDebug("Discarding changes in UserModalState.");
                // Revert VMs to original
                State.ProfileVm = State.ProfileVmOriginal with { };
                State.MembershipsVm = State.MembershipsVmOriginal with { };
                State.SometimesViewModel = State.SometimesViewModelOriginal with { };
                await Task.CompletedTask;
            }
        }
    }
}
