namespace BitUiNavigation.Client.Pages.Modals;
public interface IModalSectionGuard
{
    Task<bool> CanNavigateAwayAsync();
}
public interface ISupportsSaveOnNavigate
{
    Task SaveOnNavigateAsync();

}
public delegate void ModalGuardRegistration(IModalSectionGuard component);
