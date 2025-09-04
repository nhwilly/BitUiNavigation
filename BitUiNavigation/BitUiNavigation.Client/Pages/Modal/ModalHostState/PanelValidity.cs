namespace BitUiNavigation.Client.Services;

public sealed partial class ModalHostState
{
    public record PanelValidity(bool IsValid, int ErrorCount);

}
