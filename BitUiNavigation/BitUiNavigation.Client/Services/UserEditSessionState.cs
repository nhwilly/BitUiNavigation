using BitUiNavigation.Client.Pages.UserProfile;
using TimeWarp.State;

namespace BitUiNavigation.Client.Services;
public sealed partial class UserEditSessionState : State<UserEditSessionState>
{
    public UserDto? Entity { get; private set; }
    public UserDto? OriginalEntity { get; private set; }

    public UserProfileViewModel ProfileVm { get; private set; } = new();
    public UserMembershipsViewModel MembershipsVm { get; private set; } = new();

    public bool IsDirty => Entity is not null &&
                           OriginalEntity is not null &&
                           Entity != OriginalEntity;

    public bool IsLoading { get; private set; }
    public override void Initialize()
    {
        // NOP – session is started via actions
    }

    private void MapDtoToViewModel()
    {
        if (Entity is null) return;

        ProfileVm = new UserProfileViewModel
        {
            FirstName = Entity.FirstName,
            LastName = Entity.LastName,
            UpdatedAt = Entity.UpdatedAt.ToString("O")
        };

        // TODO: Map other panel VMs from Entity as needed
    }

    private void MapViewModelToDto()
    {
        if (Entity is null) return;

        Entity = Entity with
        {
            FirstName = ProfileVm.FirstName,
            LastName = ProfileVm.LastName
        };

        // TODO: include additional viewmodel → entity mappings
    }

    private void Commit(UserDto dto)
    {
        Entity = dto;
        OriginalEntity = dto with { };
    }
}

public partial class UserEditSessionState
{
    public static class BeginUserEditSessionActionSet
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
            private readonly ILogger<UserEditSessionState> _logger;
            private UserEditSessionState State => Store.GetState<UserEditSessionState>();

            public Handler(
                IStore store,
                ILogger<UserEditSessionState> logger,
                UserService userService)
                : base(store)
            {
                _logger = logger;
                _userService = userService;
            }

            public override async Task Handle(Action action, CancellationToken cancellationToken)
            {
                _logger.LogDebug("Initializing UserEditSession for AccountId={AccountId}, LocationId={LocationId}",
                    action.AccountId, action.LocationId);
                State.IsLoading = true;
                // Load DTO
                var dto = await _userService.GetUserAsync(action.AccountId.ToString());

                // Initialize state
                State.Commit(dto);
                State.MapDtoToViewModel();
                State.IsLoading = false;
            }
        }
    }

    public static class SaveUserActionSet
    {
        public sealed class Action : IAction { }

        public sealed class Handler : ActionHandler<Action>
        {
            private readonly UserService _userService;
            private readonly ILogger<UserEditSessionState> _logger;
            private UserEditSessionState State => Store.GetState<UserEditSessionState>();

            public Handler(
                IStore store,
                ILogger<UserEditSessionState> logger,
                UserService userService)
                : base(store)
            {
                _logger = logger;
                _userService = userService;
            }

            public override async Task Handle(Action action, CancellationToken cancellationToken)
            {
                if (State.Entity is null)
                {
                    _logger.LogWarning("Entity is null – save aborted.");
                    return;
                }

                _logger.LogDebug("Saving user {LastName}", State.Entity.LastName);

                // Map VMs → Entity, then save
                State.MapViewModelToDto();

                var saved = await _userService.SaveUserAsync(State.Entity);

                // Update state (Entity + Original + VM)
                State.Commit(saved);
                State.MapDtoToViewModel();
            }
        }
    }
    public static class SetIsLoadingActionSet
    {
        public sealed class Action : IAction
        {
            public bool IsLoading { get; private set; }
            public Action(bool isLoading)
            {
                IsLoading = isLoading;
            }
        }

        public sealed class Handler : ActionHandler<Action>
        {
            private readonly ILogger<UserEditSessionState> _logger;
            private UserEditSessionState State => Store.GetState<UserEditSessionState>();

            public Handler(
                IStore store,
                ILogger<UserEditSessionState> logger)
                : base(store)
            {
                _logger = logger;
            }

            public override async Task Handle(Action action, CancellationToken cancellationToken)
            {
                State.IsLoading = action.IsLoading;
                _logger.LogDebug("Set IsLoading to {IsLoading}", action.IsLoading);
                await Task.CompletedTask; // Simulate async operation if needed
            }
        }
    }

}