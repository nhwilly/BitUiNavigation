// NEW: Capability contract for providers that support Save/Reset and a single gate for saving.
namespace BitUiNavigation.Client.Pages.Modal.Abstract;
public interface IModalHasChanged
{
    bool HasChanged { get; }
}

// UserModelProvider implements IModalSave and IModalReset
// IModalProvider does not implement anything - use cast to get capabilities
// UserModalProvider returns UserModalState.CanSave value
// ModalHost knows the provider instance and can use the interfaces to call Save/Reset and show/hide buttons
