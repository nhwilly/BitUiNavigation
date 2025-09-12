namespace BitUiNavigation.Client.ModalHost.Abstract;

public abstract class ModalProviderBase : IModalProvider
{
    public abstract string ProviderName { get; }
    public abstract string DefaultPanel { get; }
    public virtual string Height => "640px";
    public virtual string Width => "900px";
    public virtual string MinWidth => "350px";
    public virtual string MaxWidth => "1200px";
    public abstract bool HasUnsavedChanges { get; }

    public abstract string ProviderTitle { get; }

    /// <summary>
    /// Presumes auto-save is supported unless overridden.  Note: In order to report a modal entity
    /// auto-save support result, the provider must report that value by overriding this method.
    /// If the <i>provider</i> does not support auto-save, it should return a result indicating that.
    /// </summary>
    public virtual AutoSaveSupportResult AutoSaveSupportResult { get; } = new AutoSaveSupportResult(true);
    protected readonly IStore Store;
    protected readonly ILogger _logger;

    protected ModalProviderBase(IStore store, ILogger logger)
    {
        Store = store;
        _logger = logger;
    }

    protected ModalHostState ModalHostState => Store.GetState<ModalHostState>();

    protected abstract Dictionary<string, Type> PanelMap { get; }

    public abstract Task ClearState(CancellationToken ct);
    public virtual IReadOnlyList<string> ExpectedPanelKeys => [.. PanelMap.Keys.Select(k => Normalize(k, DefaultPanel))];

    public virtual Task OnModalOpenedAsync(CancellationToken ct) => Task.CompletedTask;

    public virtual Task OnModalOpeningAsync(CancellationToken ct) => Task.CompletedTask;

    public abstract Task BuildNavSections(NavigationManager nav, CancellationToken ct);

    protected static string BuildPanelRelativeUrl(NavigationManager nav, string panelName)
    {
        var absolute = nav.GetUriWithQueryParameter("panel", panelName);
        return "/" + nav.ToBaseRelativePath(absolute);
    }

    public virtual RouteData BuildRouteData(string panelName)
    {
        var key = Normalize(panelName, DefaultPanel);
        var type = PanelMap.TryGetValue(key, out var t) ? t : typeof(NotFoundPanel);
        return new RouteData(type, new Dictionary<string, object?>());
    }

    protected virtual bool MissingPanelValidityBlocksClose => false;

    public virtual Task<bool> CanCloseAsync(CancellationToken ct) => Task.FromResult(true);

    public async Task AddValidationToSections(List<NavSection> navSections, CancellationToken ct)
    {

        foreach (var section in navSections)
        {
            AddValidationIndicators(section.CustomNavItems);
        }
        await ModalHostState.SetNavSections(navSections, ct);

    }
    private void AddValidationIndicators(List<CustomNavItem> navItems)
    {
        // Snapshot to avoid repeated state reads
        var host = Store.GetState<ModalHostState>();

        // Get this provider's panel-validity map, if any
        host.Validity.TryGetValue(ProviderName, out var providerValidity);

        foreach (var navItem in navItems)
        {
            if (navItem is null || string.IsNullOrWhiteSpace(navItem.Key)) continue;

            var panelName = Normalize(navItem.Key, DefaultPanel);

            // Safe: only enters when providerValidity is not null AND the panelName exists.
            if (providerValidity?.TryGetValue(panelName, out var panelValidity) == true && panelValidity is { IsValid: false })
            {
                var validationText = "validation error".ToQuantity(panelValidity.ErrorCount, ShowQuantityAs.Words);
                var text = $"{navItem.Text} has {validationText}";
                navItem.InvalidErrorCount = panelValidity.ErrorCount;
                navItem.Title = text;
                navItem.AriaLabel = text;
            }
            else
            {
                navItem.Title = navItem.Text;
                navItem.AriaLabel = navItem.AriaLabel;
                navItem.InvalidErrorCount = 0;
            }
        }
    }

    public virtual Task<ValidationResult> ValidateProvider(CancellationToken ct)
        => Task.FromResult<ValidationResult>(new ValidationResult());


}

