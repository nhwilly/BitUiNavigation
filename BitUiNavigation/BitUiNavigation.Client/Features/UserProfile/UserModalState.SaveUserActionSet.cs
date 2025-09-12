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
            private UserModalState UserModalState => Store.GetState<UserModalState>();
            private ModalHostState ModalHostState => Store.GetState<ModalHostState>();
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
                if (UserModalState.User is null)
                {
                    _logger.LogWarning("Entity is null – save aborted.");
                    return;
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogWarning("SaveUser canceled before start.");
                    return;
                }

                try
                {
                    _logger.LogDebug("Saving user {LastName}", UserModalState.User.LastName);

                    // Project current VM to DTO snapshot before save
                    UserModalState.MapViewModelsToDto();

                    // Persist with the supplied token (linked to ModalHost navigation token)
                    var saved = await _userService.SaveUserAsync(UserModalState.User, cancellationToken);
                    await ModalHostState.SetModalAlertType(ModalAlertType.Error,"Something went wrong");
                    if (cancellationToken.IsCancellationRequested)
                    {
                        _logger.LogWarning("SaveUser canceled after service call.");
                        return;
                    }

                    // Use returned DTO (server truth) then rehydrate the VMs
                    UserModalState.User = saved;
                    UserModalState.MapDtoToViewModels();
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("SaveUser canceled via token.");
                }
                catch (ObjectDisposedException)
                {
                    // Happens if an internal CTS was disposed by the framework while we were awaiting
                    _logger.LogWarning("SaveUser aborted: CTS disposed.");
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
