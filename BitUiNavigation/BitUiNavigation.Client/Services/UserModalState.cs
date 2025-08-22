using BitUiNavigation.Client.Pages.UserProfile;
using TimeWarp.State;

namespace BitUiNavigation.Client.Services;

public sealed partial class UserModalState : State<UserModalState>
{
    public override void Initialize()
    {
        UserDto = new UserDto { FirstName = "", LastName = "" };
        UserProfileViewModel = new UserProfileViewModel { FirstName = "", LastName = "" };
    }
    private UserDto UserDto { get; set; } = default!;
    public UserProfileViewModel UserProfileViewModel { get; private set; } = default!;

    private void MapDtoToViewModel()
    {
        UserProfileViewModel.FirstName = UserDto?.FirstName ?? "";
        UserProfileViewModel.LastName = UserDto?.LastName ?? "";
    }
    private void MapViewModelToDto()
    {
        UserDto.FirstName = UserProfileViewModel.FirstName;
        UserDto.LastName = UserProfileViewModel.LastName;
    }
    public static class GetUserActionSet
    {
        public sealed class Action : IAction
        {
            public string UserId { get; }
            public Action(string userId)
            {
                UserId = userId;
            }
        }
        public sealed class Handler : ActionHandler<Action>
        {
            private readonly ILogger<UserModalState> _logger;
            private UserModalState UserModalState => Store.GetState<UserModalState>();
            private readonly UserService _userService;
            public Handler(IStore store, ILogger<UserModalState> logger, UserService userService) : base(store)
            {
                _logger = logger;
                _userService = userService;
            }
            public override async Task Handle(Action action, CancellationToken cancellationToken)
            {
                _logger.LogDebug("Fetching user with ID: {UserId}", action.UserId);

                // fetch from source
                UserModalState.UserDto = await _userService.GetUserAsync(action.UserId);

                // update view model(s)
                UserModalState.MapDtoToViewModel();
                await Task.CompletedTask;
            }
        }
    }
    public static class SaveUserActionSet
    {
        public sealed class Action : IAction
        {
            public Action() { }
        }
        public sealed class Handler : ActionHandler<Action>
        {
            private readonly ILogger<UserModalState> _logger;
            private UserModalState UserModalState => Store.GetState<UserModalState>();
            private readonly UserService _userService;
            public Handler(IStore store, ILogger<UserModalState> logger, UserService userService) : base(store)
            {
                _logger = logger;
                _userService = userService;
            }
            public override async Task Handle(Action action, CancellationToken cancellationToken)
            {
                _logger.LogDebug("Saving user with ID: {UserId}", UserModalState.UserDto?.LastName);
                if (UserModalState.UserDto is null)
                {
                    _logger.LogWarning("UserDto is null, cannot save user.");
                    return;
                }
                // create new dto from view model(s)
                UserModalState.MapViewModelToDto();
                // refresh dto and view model(s)
                var dto = await _userService.SaveUserAsync(UserModalState.UserDto);
                UserModalState.UserDto = dto;
                UserModalState.MapDtoToViewModel();
                await Task.CompletedTask;
            }
        }
    }
}

