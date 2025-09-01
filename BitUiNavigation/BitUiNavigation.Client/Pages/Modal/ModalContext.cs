namespace BitUiNavigation.Client.Pages.Modal;

/// <summary>
/// Is used for cascading the context of which provider/panel is hosting the current panel.
/// This allows each panel to know where it is and therefore it can be validated.
/// </summary>
public sealed class ModalContext
{
    public required string ProviderKey { get; init; } 
    public required string PanelName { get; init; } 
}
