using System.Runtime.CompilerServices;
using BitUiNavigation.Client.Pages.UserProfile;
using BitUiNavigation.Client.Pages.UserProfile.Memberships;
using BitUiNavigation.Client.Pages.UserProfile.Profile;
using BitUiNavigation.Client.Pages.UserProfile.Sometimes;
using TimeWarp.State;

namespace BitUiNavigation.Client.Services;
public sealed partial class UserModalState : State<UserModalState>
{
    public UserDto? User { get; private set; }

    public UserProfileViewModel ProfileVm { get; private set; } = new();
    public UserMembershipsViewModel MembershipsVm { get; private set; } = new();
    private UserProfileViewModel ProfileVmOriginal { get; set; } = new();
    private UserMembershipsViewModel MembershipsVmOriginal { get; set; } = new();

    public SometimesViewModel SometimesViewModel { get; private set; } = new();
    private SometimesViewModel SometimesViewModelOriginal { get; set; } = new();
    private readonly Dictionary<Type, bool> _lastKnownByType = [];
    public IReadOnlyDictionary<Type, bool> LastKnownValidityByType => _lastKnownByType;

    public string ProviderTitle => User is null ? "User" : $"User: {User.FirstName} {User.LastName}";
    public override void Initialize() { }

    public bool CanSave => HasChanged;
    public bool CanReset => HasChanged;
    public bool IsSaving => true;
    public bool IsResetting => true;
    public bool SaveOnCloseEnabled => true;
    public bool IsToggled { get; private set; }
    public bool IsInitializing { get; private set; }

    public bool HasChanged
    {
        get
        {
            return
                ProfileVm != ProfileVmOriginal ||
                MembershipsVm != MembershipsVmOriginal ||
                SometimesViewModel != SometimesViewModelOriginal;
        }
    }

    private void MapDtoToViewModel()
    {
        if (User is null) return;

        ProfileVm = new UserProfileViewModel
        {
            FirstName = User.FirstName,
            LastName = User.LastName,
            UpdatedAt = User.UpdatedAt.ToString("O")
        };

        MembershipsVm = new UserMembershipsViewModel { Name = User.Name };
        // TODO: Map other panel VMs from Entity as needed
    }

    private void MapViewModelToDto()
    {
        if (User is null) return;

        User = User with
        {
            FirstName = ProfileVm.FirstName,
            LastName = ProfileVm.LastName,
            Name = MembershipsVm.Name,
        };

        // TODO: include additional view model → entity mappings
    }

    public static class SetNavItemToggleActionSet
    {
        public sealed class Action : IAction
        {
            public bool IsToggled { get; }
            public Action(bool isToggled)
            {
                IsToggled = isToggled;
            }
        }
        public sealed class Handler : ActionHandler<Action>
        {
            private readonly ILogger<UserModalState> _logger;
            private readonly IModalProvider _provider;
            private readonly NavigationManager _nav;
            private UserModalState State => Store.GetState<UserModalState>();
            public Handler(
                IStore store,
                IServiceProvider serviceProvider,
                ILogger<UserModalState> logger,
                NavigationManager nav)
                : base(store)
            {
                _provider = serviceProvider.GetRequiredKeyedService<IModalProvider>("User");
                _logger = logger;
                _nav = nav;
            }
            private UserModalState UserModalState => Store.GetState<UserModalState>();
            public override async Task Handle(Action action, CancellationToken cancellationToken)
            {
                UserModalState.IsToggled = !UserModalState.IsToggled;
                await _provider.BuildNavSections(_nav, cancellationToken);
            }
        }
    }
    public static class InitializeActionSet
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
                    var dto = await _userService.GetUserAsync(action.AccountId.ToString());
                    UserState.MapDtoToViewModel();
                }
                catch (Exception ex)
                {
                    // put up toast here...
                    _logger.LogError("Exception: {Message}", ex.Message);
                }
            }
        }
    }

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
                State.IsInitializing = action.IsInitializing;
                _logger.LogDebug("Set IsInitializing to {IsInitializing}", action.IsInitializing);
                await Task.CompletedTask;
            }
        }
    }
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

                // Map VMs → Entity, then save
                State.MapViewModelToDto();

                var saved = await _userService.SaveUserAsync(State.User);

                // Update state (Entity + Original + VM)
                // State.Commit(saved);
                State.MapDtoToViewModel();
            }
        }
    }
}
