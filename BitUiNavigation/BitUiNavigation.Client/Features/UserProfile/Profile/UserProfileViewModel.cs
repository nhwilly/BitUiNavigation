using BitUiNavigation.Client.ModalHost.Abstract;

namespace BitUiNavigation.Client.Features.UserProfile.Profile;
public record UserProfileViewModel : BaseRecord
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string UpdatedAt { get; set; } = string.Empty;
}
