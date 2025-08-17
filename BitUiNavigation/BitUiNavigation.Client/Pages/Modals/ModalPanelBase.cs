using BitUiNavigation.Client.Pages.Modals;
using FluentValidation;
using Microsoft.AspNetCore.Components;
using TimeWarp.State;

public abstract class ModalPanelBase<TModel> : 
    TimeWarpStateComponent,
    IModalSectionGuard,
    ISupportsSaveOnNavigate
    where TModel : BaseRecord
{
    [CascadingParameter] public ModalGuardRegistration? RegisterGuard { get; set; }
    [Inject] private IValidator<TModel>? Validator { get; set; } = default!;

    protected abstract TModel ViewModelFromState { get; }

    protected TModel Model => ViewModelFromState;

    protected override Task OnInitializedAsync()
    {
        RegisterGuard?.Invoke(this);
        return base.OnInitializedAsync();
    }

    protected virtual async Task<bool> ValidateModel()
    {
        if (Validator is null)
            return true;

        var result = await Validator.ValidateAsync(Model);
        return result.IsValid;
    }

    // ✔ between panels → validate only if you want, but no blocking
    public virtual Task<bool> CanNavigateToAnotherSectionAsync() => Task.FromResult(true);

    // ✔ closing modal → block if invalid
    public virtual async Task<bool> CanCloseModalAsync()
        => await ValidateModel();

    // ✔ save-on-navigate (or on close)
    public virtual async Task SaveOnNavigateAsync()
    {
        // Here we'd normally map VM → Entity and mark dirty (via state action)
        // For now we simply validate and assume mapping will occur in save action.
        await ValidateModel();
    }
}
