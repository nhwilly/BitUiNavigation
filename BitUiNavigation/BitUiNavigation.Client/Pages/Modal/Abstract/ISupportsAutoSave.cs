using BitUiNavigation.Client.Pages.Modal.Helpers;

namespace BitUiNavigation.Client.Pages.Modal.Abstract;

public interface ISupportsAutoSave
{
    //AutoSaveSupportedType AutoSaveSupported { get; }
    AutoSaveSupportResult AutoSaveSupportResult { get; }

}
