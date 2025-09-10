namespace BitUiNavigation.Client.ModalHost.Abstract;

public interface IBeforeCloseHook
{
    Task<bool> OnBeforeCloseAsync(CancellationToken ct);
}
