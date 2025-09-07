namespace BitUiNavigation.Client.Pages.ModalHost.Abstract;

public interface IBeforeCloseHook
{
    Task<bool> OnBeforeCloseAsync(CancellationToken ct);
}
