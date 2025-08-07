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

    protected TModel Model { get; set; } = default!;
    private TModel OriginalModel = default!;
    protected bool IsModelReady { get; private set; }

    protected abstract AbstractValidator<TModel> Validator { get; }
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

    protected abstract Task<TModel> CreateInitialModel();
    protected abstract Task PersistAsync(TModel model);

    protected virtual async Task<bool> ValidateModel()
    {
        var result = await Validator.ValidateAsync(Model);
        return result.IsValid;
    }

    public async Task<bool> CanNavigateAwayAsync() => !HasChanges() || (await ValidateModel());

    public async Task SaveOnNavigateAsync()
    {
        if (!HasChanges()) return;
        if (!await ValidateModel()) return;
        await ModalHostState.SetSaving();
        await PersistAsync(Model);
        OriginalModel = Model with { };
        await ModalHostState.SetSaved();
    }

    protected async Task Save()
    {
        if (!HasChanges()) return;
        if (!await ValidateModel()) return;

        await PersistAsync(Model);
        OriginalModel = Model with { };
    }
    protected bool HasChanges() => Model != OriginalModel;
}
