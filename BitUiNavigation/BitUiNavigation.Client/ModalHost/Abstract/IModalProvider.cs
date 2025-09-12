using FluentValidation.Results;

namespace BitUiNavigation.Client.ModalHost.Abstract;

public interface IModalProvider
{
    string ProviderName { get; }
    string DefaultPanel { get; }
    string Width { get; }
    string Height { get; }
    string MinWidth { get; }
    string MaxWidth { get; }
    string ProviderTitle { get; }

    AutoSaveSupportResult AutoSaveSupportResult { get; }

    IReadOnlyList<string> ExpectedPanelKeys { get; }

    Task BuildNavSections(NavigationManager nav, CancellationToken ct);

    /// <summary>
    /// Maps the current panel key to a RouteData (component type + parameters) for the right panel.
    /// </summary>
    RouteData BuildRouteData(string panelKey);
    Task OnModalOpenedAsync(CancellationToken ct);
    Task OnModalOpeningAsync(CancellationToken ct);

    Task<bool> CanCloseAsync(CancellationToken ct);

    public virtual Task<ValidationResult> ValidateProvider(CancellationToken ct)
    => Task.FromResult<ValidationResult>(new ValidationResult());

    //void AddValidationIndicators(List<CustomNavItem> items);
    Task AddValidationToSections(List<NavSection> navSections, CancellationToken ct);
    bool HasUnsavedChanges { get; }

    Task ClearState(CancellationToken ct);

}