// NEW: Capability contract for providers that support Save/Reset and a single gate for saving.
namespace BitUiNavigation.Client.Pages.Modals;


public interface IModalSaveReset
{
    // Advertise capabilities (optional buttons)
    //bool CanSave { get; }
    //bool CanReset { get; }

    // Persistence and model restoration
    Task SaveAsync(CancellationToken ct);
    Task ResetAsync(CancellationToken ct);
}

// modal host can show save button if modalhostState can save is true and imodalprovider implements Imodalsavereset 
// modal provider decides if isDirty and can save and sends message show save button