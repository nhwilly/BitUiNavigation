using BitUiNavigation.Client.Pages.Modal.Abstract;

namespace BitUiNavigation.Client.Pages.UserProfile;
public record UserProfileViewModel : BaseRecord
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string UpdatedAt { get; set; } = string.Empty;
}
