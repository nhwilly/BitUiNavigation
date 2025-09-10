using BitUiNavigation.Client.ModalHost.Abstract;

namespace BitUiNavigation.Client.Features.UserProfile.Sometimes;

public record SometimesViewModel: BaseRecord
{
    public string Description { get; set; } = string.Empty;
}