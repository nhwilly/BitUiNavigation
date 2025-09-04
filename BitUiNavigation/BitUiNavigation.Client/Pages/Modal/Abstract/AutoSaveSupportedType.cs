namespace BitUiNavigation.Client.Pages.Modal.Abstract;

[Flags]
public enum AutoSaveSupportedType
{
    Unassigned = 1,
    Supported = 2,
    UnsupportedByModalState = 4,
    UnavailableInCurrentModalState = 8,
    UnsupportedByProvider = 16,
}
public record AutoSaveSupportResult(bool IsSupported, string? Message = null);