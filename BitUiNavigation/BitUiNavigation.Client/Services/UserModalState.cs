using BitUiNavigation.Client.Pages.UserProfile;
using TimeWarp.State;

namespace BitUiNavigation.Client.Services;
public sealed partial class UserModalState : State<UserModalState>
{
    public UserDto? Entity { get; private set; }
    public UserDto? OriginalEntity { get; private set; }

    public UserProfileViewModel ProfileVm { get; private set; } = new();
    public UserMembershipsViewModel MembershipsVm { get; private set; } = new();
    private UserProfileViewModel ProfileVmOriginal { get; set; } = new();
    private UserMembershipsViewModel MembershipsVmOriginal { get; set; } = new();

    private readonly Dictionary<Type, bool> _lastKnownByType = [];
    public IReadOnlyDictionary<Type, bool> LastKnownValidityByType => _lastKnownByType;

    public string ProviderTitle => Entity is null ? "User" : $"User: {Entity.FirstName} {Entity.LastName}";
    public override void Initialize() { }

    public bool CanSave => HasChanged;
    public bool CanReset => HasChanged;
    public bool IsSaving => true;
    public bool IsResetting => true;
    public bool SaveOnCloseEnabled => true;

    public bool IsLoading { get; private set; }

    public bool HasChanged
    {
        get
        {
            return
                ProfileVm != ProfileVmOriginal || 
                MembershipsVm != MembershipsVmOriginal;
        }
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

        MembershipsVm = new UserMembershipsViewModel { Name = Entity.Name };
        // TODO: Map other panel VMs from Entity as needed
    }

    private void MapViewModelToDto()
    {
        if (Entity is null) return;

        Entity = Entity with
        {
            FirstName = ProfileVm.FirstName,
            LastName = ProfileVm.LastName,
            Name = MembershipsVm.Name,
        };

        // TODO: include additional view model → entity mappings
    }

    private void Commit(UserDto dto)
    {
        Entity = dto;
        OriginalEntity = dto with { };
    }

    //public static class SetValidityActionSet
    //{
    //    public sealed class Action : IAction
    //    {
    //        public Type PanelType { get; }
    //        public bool IsValid { get; }
    //        public Action(Type panelType, bool isValid)
    //        {
    //            PanelType = panelType;
    //            IsValid = isValid;
    //        }
    //    }
    //    public sealed class Handler : ActionHandler<Action>
    //    {
    //        private readonly ILogger<UserModalState> _logger;
    //        private UserModalState State => Store.GetState<UserModalState>();
    //        public Handler(
    //            IStore store,
    //            ILogger<UserModalState> logger)
    //            : base(store)
    //        {
    //            _logger = logger;
    //        }
    //        public override async Task Handle(Action action, CancellationToken cancellationToken)
    //        {
    //            State.LastKnownValidityByType.TryGetValue(action.PanelType, out var existing);
    //            if (existing != action.IsValid)
    //            {
    //                State._lastKnownByType[action.PanelType] = action.IsValid;
    //                _logger.LogDebug("Set validity for {PanelType} to {IsValid}", action.PanelType.Name, action.IsValid);
    //            }
    //            await Task.CompletedTask; // Simulate async operation if needed
    //        }
    //    }
    //}
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
                try
                {
                    _logger.LogDebug("Initializing UserEditSession for AccountId={AccountId}, LocationId={LocationId}",
                        action.AccountId, action.LocationId);
                    //await State.SetIsLoading(true);
                    // Load DTO
                    var dto = await _userService.GetUserAsync(action.AccountId.ToString());

                    // Initialize state
                    State.Commit(dto);
                    State.MapDtoToViewModel();

                }
                catch (Exception ex)
                {
                    // put up toast here...
                    _logger.LogError("Exception: {Message}", ex.Message);
                }
                finally
                {
                    //await State.SetIsLoading(false);
                }
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
                State.IsLoading = action.IsLoading;
                _logger.LogDebug("Set IsLoading to {IsLoading}", action.IsLoading);
                await Task.CompletedTask; // Simulate async operation if needed
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
}
