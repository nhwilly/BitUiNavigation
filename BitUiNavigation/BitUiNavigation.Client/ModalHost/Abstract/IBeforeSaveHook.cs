namespace BitUiNavigation.Client.ModalHost.Abstract;

public interface IBeforeSaveHook
{
    Task<bool> OnBeforeSaveAsync(CancellationToken ct);
}
