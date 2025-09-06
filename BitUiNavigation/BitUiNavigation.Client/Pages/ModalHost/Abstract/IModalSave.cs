namespace BitUiNavigation.Client.Pages.ModalHost.Abstract;

public interface IModalSave
{
    bool CanSave { get; }
    Task SaveAsync(CancellationToken ct);
}
