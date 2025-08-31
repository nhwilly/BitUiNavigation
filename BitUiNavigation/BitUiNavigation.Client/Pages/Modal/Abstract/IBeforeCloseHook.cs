namespace BitUiNavigation.Client.Pages.Modal.Abstract;

public interface IBeforeCloseHook
{
    Task<bool> OnBeforeCloseAsync(CancellationToken ct);
}
