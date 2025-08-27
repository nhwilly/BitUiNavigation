namespace BitUiNavigation.Client.Pages.Modals;

public interface IBeforeCloseHook
{
    Task<bool> OnBeforeCloseAsync(CancellationToken ct);
}
