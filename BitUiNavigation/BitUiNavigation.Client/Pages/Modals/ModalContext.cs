namespace BitUiNavigation.Client.Pages.Modals;

public sealed class ModalContext
{
    public required string ProviderKey { get; init; } // e.g., nameof(UserModalProvider)
    public required string PanelName { get; init; } // e.g., nameof(UserProfilePanel)
}
