namespace BitUiNavigation.Client.Pages.Modal;

public sealed class ModalContext
{
    public required string ProviderKey { get; init; } // e.g., nameof(UserModalProvider)
    public required string PanelName { get; init; } // e.g., nameof(UserProfilePanel)
}
