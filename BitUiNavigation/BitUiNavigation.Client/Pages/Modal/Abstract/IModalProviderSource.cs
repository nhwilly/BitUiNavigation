namespace BitUiNavigation.Client.Pages.Modal.Abstract;

public interface IModalProviderSource
{
    IEnumerable<IModalProvider> GetModalProviders();
}
