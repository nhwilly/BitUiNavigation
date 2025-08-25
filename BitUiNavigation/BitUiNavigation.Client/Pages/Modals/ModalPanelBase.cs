using FluentValidation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeWarp.State;

namespace BitUiNavigation.Client.Pages.Modals;
public abstract class ModalPanelBase<TModel> :
    TimeWarpStateComponent,
    IModalPanel,
    ISupportsSaveOnNavigate
    where TModel : BaseRecord
{
    [CascadingParameter] public ModalPanelRegistration? RegisterPanel { get; set; }
    [Inject] private IValidator<TModel>? Validator { get; set; } = default!;
    [CascadingParameter] public IModalPanelRegistry? PanelRegistry { get; set; }

    protected override Task OnInitializedAsync()
    {
        PanelRegistry?.Register(this);
        return base.OnInitializedAsync();
    }

    public override void Dispose()
    {
        PanelRegistry?.Unregister(this);
        base.Dispose();

    }

    /// <summary>
    /// Provides a reference to the model used by the derived panel.
    /// </summary>
    protected abstract TModel Model { get; }

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
        var editContextValid = ShowAllValidationErrors();
        if (Validator is null)
        {
            // Registry already updated by ShowAllValidationErrors
            Console.WriteLine($"Validator for {this.GetType().Name} is null...");
            return editContextValid;
        }
        else
        {
            Console.WriteLine($"Using validator for {Validator.GetType().Name}");
        }
        var fv = await Validator.ValidateAsync(Model);
        var finalValid = editContextValid && fv.IsValid;
        Console.WriteLine($"Validator result for {this.GetType().Name} is {finalValid}...");

        // Update the registry with the combined truth
        PanelRegistry?.SetValidity(this, finalValid);

        return finalValid;
        //var result = await Validator.ValidateAsync(Model);
        //PanelRegistry?.SetValidity(this, result.IsValid); // <— report validity
        //return result.IsValid;
    }

    protected EditForm editForm = default!;
    private bool ShowAllValidationErrors()
    {
        var editContext = editForm?.EditContext;
        if (editContext == null)
        {
            // No form → treat as valid (or skip touching the registry)
            PanelRegistry?.SetValidity(this, true);
            return true;
        }

        // Mark all properties as having been interacted with
        MarkAllFieldsAsModified(editContext);

        // Validate the form
        var isValid = editContext.Validate();

        PanelRegistry?.SetValidity(this, isValid);
        return isValid;

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

    // ✔ between panels → validate only if you want, but no blocking
    public virtual Task<bool> CanNavigateToAnotherSectionAsync() => Task.FromResult(true);

    // ✔ closing modal → block if invalid
    public virtual async Task<bool> CanCloseModalAsync()
    {
        var isValid = await ValidateModel();
        Console.WriteLine($"CanCloseModalAsync - {editForm?.Model?.GetType().Name} isValid: {isValid}");
        return isValid;
    }

    // ✔ save-on-navigate (or on close)
    public virtual async Task SaveOnNavigateAsync()
    {
        // Here we'd normally map VM → Entity and mark dirty (via state action)
        // For now we simply validate and assume mapping will occur in save action.
        var isValid = await ValidateModel();
        Console.WriteLine($"SaveOnNavigateAsync - {editForm?.Model?.GetType().Name} isValid: {isValid}");
    }
}
