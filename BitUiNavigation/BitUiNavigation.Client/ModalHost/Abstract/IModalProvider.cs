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
    public List<NavSection> NavSections { get; }

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

    //Task<(bool IsValid, string generalMessage, IReadOnlyList<string> Messages)> ValidateProviderAsync(CancellationToken ct);
    public virtual Task<ValidationResult> ValidateProvider(CancellationToken ct)
    => Task.FromResult<ValidationResult>(new ValidationResult());

    void AddValidationIndicators(List<CustomNavItem> items);
    void AddValidationToSections();
    bool HasUnsavedChanges { get; }

    Task ClearState(CancellationToken ct);

}