using BitUiNavigation.Client.ModalHost.Components;
using BitUiNavigation.Client.ModalHost.Helpers;
using FluentValidation.Results;

namespace BitUiNavigation.Client.ModalHost.Abstract;

public abstract class ModalProviderBase : IModalProvider
{
    public abstract string ProviderName { get; }
    public abstract string DefaultPanel { get; }
    public virtual string Width => "900px";
    public virtual string MinWidth => "350px";
    public virtual string MaxWidth => "1200px";
    public virtual string Height => "640px";

    public abstract string InstanceName { get; }

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

    protected ModalHostState HostState => Store.GetState<ModalHostState>();

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

    public void AddValidationIndicators(List<CustomNavItem> items)
    {
        // Snapshot to avoid repeated state reads
        var host = Store.GetState<ModalHostState>();

        // Get this provider's panel-validity map, if any
        host.Validity.TryGetValue(ProviderName, out var perPanel);

        foreach (var item in items)
        {
            if (item is null || string.IsNullOrWhiteSpace(item.Key)) continue;

            var panelKey = Normalize(item.Key, DefaultPanel);

            // Safe: only enters when perPanel is not null AND the key exists.
            if (perPanel?.TryGetValue(panelKey, out var pv) == true && pv is { IsValid: false })
            {
                item.ValidationIconName = BitIconName.CriticalErrorSolid;

                var msg = pv.ErrorCount == 1
                    ? "a validation error"
                    : $"{pv.ErrorCount} validation errors";

                // Optional a11y/tooltip text
                item.Title = string.IsNullOrWhiteSpace(item.Title)
                    ? $"Has {msg}"
                    : $"{item.Title} has {msg}";

                item.AriaLabel = $"{item.Text} has {msg}";
            }
            else
            {
                // leave as-is for valid/unknown
                // If you want to clear or set an "ok" icon, do it here.
                item.ValidationIconName = null;
            }
        }
    }

    public virtual Task<ValidationResult> ValidateProvider(CancellationToken ct)
        => Task.FromResult<ValidationResult>(new ValidationResult());

    public abstract bool HasUnsavedChanges { get; }

}

