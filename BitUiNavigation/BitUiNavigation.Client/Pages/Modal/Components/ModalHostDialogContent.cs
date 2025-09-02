namespace BitUiNavigation.Client.Pages.Modal.Components;

public record ModalHostDialogContent
{
    public required string Title { get; init; } 
    public required string Body { get; init; } = string.Empty;
    public string OkButtonText { get; init; } = "OK";
    public string CancelButtonText { get; init; } = "Cancel";
    public string SaveButtonText { get; init; } = "Save";
    public string DiscardButtonText { get; init; } = "Discard";
    public string ResetButtonText { get; init; } = "Reset";
    public bool ShowOkButton { get; init; } = false;
    public bool ShowCancelButton { get; init; } = false;
    public bool ShowSaveButton { get; init; } = false;
    public bool ShowDiscardButton { get; init; } = false;
    public bool ShowResetButton { get; init; } = false;
    public string IconName { get; init; } = BitIconName.InfoSolid;
    public EventCallback ActionSelected { get; init; } = EventCallback.Empty;
}
