using BitUiNavigation.Client.Pages.ModalHost.Abstract;

namespace BitUiNavigation.Client.Pages.UserProfile.Sometimes;

public record SometimesViewModel: BaseRecord
{
    public string Description { get; set; } = string.Empty;
}