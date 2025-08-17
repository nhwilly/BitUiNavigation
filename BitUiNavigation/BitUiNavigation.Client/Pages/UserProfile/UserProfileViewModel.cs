using BitUiNavigation.Client.Pages.Modals;

namespace BitUiNavigation.Client.Pages.UserProfile;
public record UserProfileViewModel : BaseRecord
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string UpdatedAt { get; set; } = string.Empty;
}
