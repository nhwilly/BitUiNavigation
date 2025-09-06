namespace BitUiNavigation.Client.Pages.ModalHost.State;

public sealed partial class ModalHostState
{
    public record PanelValidity(bool IsValid, int ErrorCount);

}
