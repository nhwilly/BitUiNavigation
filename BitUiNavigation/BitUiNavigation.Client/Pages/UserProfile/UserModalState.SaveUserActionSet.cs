namespace BitUiNavigation.Client.Pages.UserProfile;
public sealed partial class UserModalState
{
    public static class SaveUserActionSet
    {
        public sealed class Action : IAction { }

        public sealed class Handler : ActionHandler<Action>
        {
            private readonly UserService _userService;
            private readonly ILogger<UserModalState> _logger;
            private UserModalState State => Store.GetState<UserModalState>();

            public Handler(
                IStore store,
                ILogger<UserModalState> logger,
                UserService userService)
                : base(store)
            {
                _logger = logger;
                _userService = userService;
            }

            public override async Task Handle(Action action, CancellationToken cancellationToken)
            {
                if (State.User is null)
                {
                    _logger.LogWarning("Entity is null – save aborted.");
                    return;
                }

                _logger.LogDebug("Saving user {LastName}", State.User.LastName);
                State.MapViewModelToDto();
                var saved = await _userService.SaveUserAsync(State.User, cancellationToken);
                State.MapDtoToViewModel();
            }
        }
    }
}
