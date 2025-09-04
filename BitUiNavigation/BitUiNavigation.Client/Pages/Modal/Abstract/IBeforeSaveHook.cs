namespace BitUiNavigation.Client.Pages.Modal.Abstract;

public interface IBeforeSaveHook
{
    Task<bool> OnBeforeSaveAsync(CancellationToken ct);
}
