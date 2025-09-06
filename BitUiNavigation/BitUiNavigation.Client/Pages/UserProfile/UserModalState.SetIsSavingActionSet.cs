namespace BitUiNavigation.Client.Pages.UserProfile;

public sealed partial class UserModalState
{
    public static class SetIsSavingActionSet
    {
        public sealed class Action : IAction
        {
            public bool IsSaving { get; }
            public Action(bool isSaving) => IsSaving = isSaving;
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
                State.IsSaving = action.IsSaving;
                _logger.LogDebug("Set IsSaving to {IsSaving}.", action.IsSaving);
                await Task.CompletedTask;
            }
        }
    }
}
