namespace BitUiNavigation.Client.Features.UserProfile;
public sealed partial class UserModalState
{
    public static class InitializeDataActionSet
    {
        public sealed class Action : IAction
        {
            public Guid AccountId { get; }
            public Guid LocationId { get; }

            public Action(Guid accountId, Guid locationId)
            {
                AccountId = accountId;
                LocationId = locationId;
            }
        }

        public sealed class Handler : ActionHandler<Action>
        {
            private readonly UserService _userService;
            private readonly ILogger<UserModalState> _logger;
            private UserModalState UserState => Store.GetState<UserModalState>();
            public Handler(IStore store, ILogger<UserModalState> logger, UserService userService) : base(store)
            {
                _logger = logger;
                _userService = userService;
            }

            public override async Task Handle(Action action, CancellationToken cancellationToken)
            {
                try
                {
                    _logger.LogDebug("Initializing UserEditSession for AccountId={AccountId}, LocationId={LocationId}",
                        action.AccountId, action.LocationId);
                    UserState.User = await _userService.GetUserAsync(action.AccountId.ToString(), cancellationToken);
                    UserState.MapDtoToViewModels();
                }
                catch (Exception ex)
                {
                    // put up toast here...
                    _logger.LogError("Exception: {Message}", ex.Message);
                }
            }
        }
    }
}
