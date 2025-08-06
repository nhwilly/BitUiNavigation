namespace BitUiNavigation.Client.Pages.Modals;
public interface IModalSectionGuard
{
    Task<bool> CanNavigateAwayAsync();
}
public delegate void ModalGuardRegistration(IModalSectionGuard component);
