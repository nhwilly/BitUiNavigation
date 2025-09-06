namespace BitUiNavigation.Client.Pages.ModalHost.Abstract;

public interface IBeforeSaveHook
{
    Task<bool> OnBeforeSaveAsync(CancellationToken ct);
}
