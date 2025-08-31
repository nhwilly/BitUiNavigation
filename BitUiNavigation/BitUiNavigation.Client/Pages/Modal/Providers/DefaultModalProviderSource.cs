using BitUiNavigation.Client.Pages.Modal.Abstract;

namespace BitUiNavigation.Client.Pages.Modal.Providers;

public sealed class DefaultModalProviderSource : IModalProviderSource
{
    private readonly IEnumerable<IModalProvider> _providers;
    public DefaultModalProviderSource(IEnumerable<IModalProvider> providers) => _providers = providers;
    public IEnumerable<IModalProvider> GetModalProviders() => _providers;
}