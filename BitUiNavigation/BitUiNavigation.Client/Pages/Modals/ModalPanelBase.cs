using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Components;

namespace BitUiNavigation.Client.Pages.Modals;
public abstract class ModalPanelBase<TModel> : ComponentBase,
    IModalSectionGuard,
    ISupportsSaveOnNavigate
    where TModel : IEquatable<TModel>
{
    [CascadingParameter] public ModalGuardRegistration? RegisterGuard { get; set; }

    protected TModel Model { get; set; } = default!;
    private TModel OriginalModel = default!;

    protected abstract AbstractValidator<TModel> Validator { get; }

    //protected override void OnInitialized()
    //{
    //    base.OnInitialized();
    //    RegisterGuard?.Invoke(this);

    //    Model = CreateInitialModel();
    //    OriginalModel = CloneModel(Model);
    //}

    protected override async Task OnInitializedAsync()
    {
        base.OnInitialized();
        RegisterGuard?.Invoke(this);

        Model = await CreateInitialModel();
        OriginalModel = CloneModel(Model);
    }

    protected abstract Task<TModel> CreateInitialModel();
    protected abstract Task PersistAsync(TModel model);

    protected virtual async Task<bool> ValidateModel()
    {
        //var result = Validator.Validate(Model);
        var result = await Validator.ValidateAsync(Model);
        return result.IsValid;
    }

    public async Task<bool> CanNavigateAwayAsync()
    {
        var canNavigate = await Task.FromResult(!HasChanges() || (await ValidateModel()));
        return canNavigate;
    }

    public async Task SaveOnNavigateAsync()
    {
        if (!HasChanges()) return;
        if (!await ValidateModel()) return;

        await PersistAsync(Model);
        OriginalModel = CloneModel(Model);
    }

    protected async Task Save()
    {
        if (!HasChanges()) return;
        if (!await ValidateModel()) return;

        await PersistAsync(Model);
        OriginalModel = CloneModel(Model);
    }

    protected bool HasChanges() => !Model.Equals(OriginalModel);
    protected TModel CloneModel(TModel model) => model switch
    {
        ICloneable c => (TModel)c.Clone()!,
        _ => JsonSerializer.Deserialize<TModel>(JsonSerializer.Serialize(model))!
    };
}
