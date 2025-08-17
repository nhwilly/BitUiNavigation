namespace BitUiNavigation.Client.Pages.Modals;
public interface IModalSectionGuard
{
    //Task<bool> CanNavigateAwayAsync();
    //Task<bool> CanNavigateToAnotherSectionAsync();
    Task<bool> CanCloseModalAsync();
}
public interface ISupportsSaveOnNavigate
{
    Task SaveOnNavigateAsync();

}
public delegate void ModalGuardRegistration(IModalSectionGuard component);
