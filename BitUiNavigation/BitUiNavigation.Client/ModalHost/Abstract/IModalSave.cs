namespace BitUiNavigation.Client.ModalHost.Abstract;

public interface IModalSave
{
    bool CanSave { get; }
    Task SaveAsync(CancellationToken ct);
    bool IsSaving { get; }
}
