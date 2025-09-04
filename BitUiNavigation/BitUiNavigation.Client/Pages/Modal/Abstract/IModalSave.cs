namespace BitUiNavigation.Client.Pages.Modal.Abstract;

public interface IModalSave
{
    bool CanSave { get; }
    Task SaveAsync(CancellationToken ct);
}
