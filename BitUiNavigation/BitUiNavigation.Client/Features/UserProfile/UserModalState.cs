namespace BitUiNavigation.Client.Features.UserProfile;
public sealed partial class UserModalState : State<UserModalState>, ISupportsAutoSave
{
    public UserDto? User { get; private set; }

    public UserProfileViewModel ProfileVm { get; private set; } = new();
    public UserMembershipsViewModel MembershipsVm { get; private set; } = new();
    private UserProfileViewModel ProfileVmOriginal { get; set; } = new();
    private UserMembershipsViewModel MembershipsVmOriginal { get; set; } = new();
    public string InstanceName => ProfileVm?.FirstName ?? string.Empty;

    /// <summary>
    /// Suggested text: "Auto save on close is not available for ${InstanceName}";
    /// </summary>
    public AutoSaveSupportResult AutoSaveSupportResult =>
        User is null
            ? new AutoSaveSupportResult(false, "Auto save is not available - no user is loaded.")
            : new AutoSaveSupportResult(true);


    public SometimesViewModel SometimesViewModel { get; private set; } = new();
    private SometimesViewModel SometimesViewModelOriginal { get; set; } = new();
    private readonly Dictionary<Type, bool> _lastKnownByType = [];
    public IReadOnlyDictionary<Type, bool> LastKnownValidityByType => _lastKnownByType;

    public string ProviderTitle => User is null ? "User" : $"User: {User.FirstName} {User.LastName}";
    public override void Initialize() { }

    public bool CanSave => HasChanged;
    public bool CanReset => HasChanged;
    public bool IsSaving { get; private set; }
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
        SometimesViewModel = new SometimesViewModel { Description = "" };

        ProfileVmOriginal = ProfileVm with { };
        MembershipsVmOriginal = MembershipsVm with { };
        SometimesViewModelOriginal = SometimesViewModel with { };
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

}
