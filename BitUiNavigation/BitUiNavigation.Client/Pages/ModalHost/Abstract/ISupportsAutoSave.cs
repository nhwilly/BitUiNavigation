using BitUiNavigation.Client.Pages.ModalHost.Helpers;

namespace BitUiNavigation.Client.Pages.ModalHost.Abstract;

public interface ISupportsAutoSave
{
    //AutoSaveSupportedType AutoSaveSupported { get; }
    AutoSaveSupportResult AutoSaveSupportResult { get; }

}
