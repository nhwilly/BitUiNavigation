using BitUiNavigation.Client.Pages.Modals;
using FluentValidation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeWarp.State;

public abstract class ModalPanelBase<TModel> :
    TimeWarpStateComponent,
    IModalPanel,
    ISupportsSaveOnNavigate
    where TModel : BaseRecord
{
    [CascadingParameter] public ModalPanelRegistration? RegisterPanel { get; set; }
    [Inject] private IValidator<TModel>? Validator { get; set; } = default!;

    /// <summary>
    /// Provides a reference to the model used by the derived panel.
    /// </summary>
    protected abstract TModel Model { get; }

    protected override Task OnInitializedAsync()
    {
        RegisterPanel?.Invoke(this);
        return base.OnInitializedAsync();
    }

    protected override Task OnParametersSetAsync()
    {
        return base.OnParametersSetAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender) await ValidateModel();
        await base.OnAfterRenderAsync(firstRender);
    }
    protected virtual async Task<bool> ValidateModel()
    {
        if (Validator is null)
            return true;
        ShowAllValidationErrors();
        var result = await Validator.ValidateAsync(Model);
        return result.IsValid;
    }
    protected EditForm editForm = default!;
    private void ShowAllValidationErrors()
    {
        var editContext = editForm?.EditContext;
        if (editContext == null) return;

        // Mark all properties as having been interacted with
        MarkAllFieldsAsModified(editContext);

        // Validate the form
        var isValid = editContext.Validate();

        // Update UI
        //StateHasChanged();

    }
    private void MarkAllFieldsAsModified(EditContext editContext)
    {
        var properties = editContext.Model.GetType().GetProperties()
            .Where(prop => prop.CanWrite && prop.CanRead);

        foreach (var property in properties)
        {
            var fieldIdentifier = new FieldIdentifier(editContext.Model, property.Name);
            editContext.NotifyFieldChanged(fieldIdentifier);
        }
    }

    private void HandleValidSubmit()
    {

    }

    private void HandleInvalidSubmit()
    {

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
