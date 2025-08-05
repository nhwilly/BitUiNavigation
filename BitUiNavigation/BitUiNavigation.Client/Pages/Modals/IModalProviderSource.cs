namespace BitUiNavigation.Client.Pages.Modals;

public interface IModalProviderSource
{
    IEnumerable<IModalProvider> GetModalProviders();
}
