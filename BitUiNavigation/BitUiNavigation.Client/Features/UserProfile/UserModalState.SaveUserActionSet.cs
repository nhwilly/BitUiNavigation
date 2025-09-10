namespace BitUiNavigation.Client.Features.UserProfile;
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

                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogDebug("SaveUser canceled before start.");
                    return;
                }

                try
                {
                    _logger.LogDebug("Saving user {LastName}", State.User.LastName);

                    // Project current VM to DTO snapshot before save
                    State.MapViewModelToDto();

                    // Persist with the supplied token (linked to ModalHost navigation token)
                    var saved = await _userService.SaveUserAsync(State.User, cancellationToken);

                    if (cancellationToken.IsCancellationRequested)
                    {
                        _logger.LogDebug("SaveUser canceled after service call.");
                        return;
                    }

                    // Use returned DTO (server truth) then rehydrate the VMs
                    State.User = saved;
                    State.MapDtoToViewModel();
                }
                catch (OperationCanceledException)
                {
                    _logger.LogDebug("SaveUser canceled via token.");
                }
                catch (ObjectDisposedException)
                {
                    // Happens if an internal CTS was disposed by the framework while we were awaiting
                    _logger.LogDebug("SaveUser aborted: CTS disposed.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error during SaveUser.");
                    throw;
                }
            }
        }
    }
}
