using BitUiNavigation.Client.Services;
using FluentValidation;
using Microsoft.AspNetCore.Components;
using TimeWarp.State;

namespace BitUiNavigation.Client.Pages.Modals;
public abstract class ModalPanelBase<TModel> : TimeWarpStateComponent,
    IModalSectionGuard,
    ISupportsSaveOnNavigate
    where TModel : BaseRecord
{
    [CascadingParameter] public ModalGuardRegistration? RegisterGuard { get; set; }
    [Inject] private IValidator<TModel>? Validator { get; set; } = default!;

    protected virtual async Task<bool> ValidateModel()
    {
        if (Validator is null)
            return true;

        var result = await Validator.ValidateAsync(Model);
        return result.IsValid;
    }
    protected TModel Model { get; set; } = default!;
    private TModel OriginalModel = default!;
    protected bool IsModelReady { get; private set; }

    protected ModalHostState ModalHostState => GetState<ModalHostState>();

    protected override async Task OnInitializedAsync()
    {
        await ModalHostState.SetModelReady(IsModelReady);

        await base.OnInitializedAsync();
        RegisterGuard?.Invoke(this);

        await ModalHostState.SetFetching(true);
        Model = await CreateInitialModel();
        OriginalModel = Model with { };
        await ModalHostState.SetFetching(false);

        await ModalHostState.SetModelReady(IsModelReady);
        IsModelReady = true;
    }
    public async Task<bool> CanNavigateToAnotherSectionAsync()
    {
        // Between panes → allow even if invalid
        // but still trigger save-on-navigate
        return await Task.FromResult(true);
    }

    public async Task<bool> CanCloseModalAsync()
    {
        // Only block if invalid when closing the modal
        if (!HasChanges()) return true;
        return await ValidateModel();
    }

    protected abstract Task<TModel> CreateInitialModel();
    protected abstract Task PersistAsync();

    /// <summary>
    /// REMOVE OR TRUE TO RETURN TO NORMAL
    /// </summary>
    /// <returns></returns>
    public async Task<bool> CanNavigateAwayAsync() => !HasChanges() || ((await ValidateModel()) || true);

    public async Task SaveOnNavigateAsync()
    {
        if (!HasChanges()) return;
        if (!await ValidateModel()) return;
        await ModalHostState.SetSaving();
        await PersistAsync();
        OriginalModel = Model with { };
        await ModalHostState.SetSaved();
    }

    protected async Task Save()
    {
        if (!HasChanges()) return;
        if (!await ValidateModel()) return;

        await PersistAsync();
        OriginalModel = Model with { };
    }
    protected bool HasChanges() => Model != OriginalModel;
}
