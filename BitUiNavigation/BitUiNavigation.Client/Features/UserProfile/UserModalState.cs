namespace BitUiNavigation.Client.Features.UserProfile;
public sealed partial class UserModalState : State<UserModalState>, ISupportsAutoSave
{
    public UserDto? User { get; private set; }

    public UserProfileViewModel ProfileVm { get; private set; } = new();
    public UserMembershipsViewModel MembershipsVm { get; private set; } = new();
    private UserProfileViewModel ProfileVmOriginal { get; set; } = new();
    private UserMembershipsViewModel MembershipsVmOriginal { get; set; } = new();
    public SometimesViewModel SometimesViewModel { get; private set; } = new();
    private SometimesViewModel SometimesViewModelOriginal { get; set; } = new();

    public string InstanceName => ProfileVm?.FirstName ?? string.Empty;

    /// <summary>
    /// Suggested text: "Auto save on close is not available for ${InstanceName}";
    /// </summary>
    public AutoSaveSupportResult AutoSaveSupportResult =>
        User is null
            ? new AutoSaveSupportResult(false, "Auto save is not available - no user is loaded.")
            : new AutoSaveSupportResult(true);


    private readonly Dictionary<Type, bool> _lastKnownByType = [];
    public IReadOnlyDictionary<Type, bool> LastKnownValidityByType => _lastKnownByType;

    public string ProviderTitle => User is null ? "User" : $"User: {User.FirstName} {User.LastName}";
    public override void Initialize() { }

    /// <summary>
    /// This optionally allows this state to prevent saving if some condition is not met.  This is particularly useful for security.
    /// </summary>
    public bool CanSave => true;
    public bool CanReset => true;

    /// <summary>
    /// Indicates that the entire state is being loaded.  Used by the UI to show work in progress.  Additional IsLoading properties can be created to track specific panels or view models which may be lazily loaded.
    /// </summary>
    public bool IsLoading { get; private set; }

    /// <summary>
    /// Indicates that there is work in progress to save changes.  Specifically blocks navigation to prevent data loss or inadvertent cancellation during lengthy calls.
    /// </summary>
    public bool IsSaving { get; private set; }
    public bool IsResetting { get; private set; }
    public bool ShouldShowSomeSpecialPanel { get; private set; }

    /// <summary>
    /// Indicates that one or more view models has properties that are not equal - relies on value equality.
    /// <i>If view models are nested or contain lists, <b>standard record equality is not enough.</b> Use Generator.Equals or some other package. </i>
    /// </summary>
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

    private void MapDtoToViewModels()
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

    private void MapViewModelsToDto()
    {
        if (User is null) return;

        User = User with
        {
            FirstName = ProfileVm.FirstName,
            LastName = ProfileVm.LastName,
            Name = MembershipsVm.Name,
        };
    }

}
