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
    [Inject] public ILogger<TModel>? Logger { get; set; }
    [Inject] private IValidator<TModel>? Validator { get; set; } = default!;
    [CascadingParameter] public IModalPanelRegistry? PanelRegistry { get; set; }
    [CascadingParameter] protected EditContext? CurrentEditContext { get; set; }
    protected EditForm editForm = default!;
    private EditContext? _subscribedCtx;

    protected override Task OnInitializedAsync()
    {
        PanelRegistry?.Register(this);
        return base.OnInitializedAsync();
    }

    public override void Dispose()
    {
        Unsubscribe();
        PanelRegistry?.Unregister(this);
        base.Dispose();
    }
    private void Subscribe(EditContext ctx)
    {
        _subscribedCtx = ctx;
        ctx.OnValidationStateChanged += HandleValidationStateChanged;
        RehydrateValidationUiIfNeeded(ctx);
        //ctx.OnFieldChanged += HandleFieldChanged; // optional if you want updates on blur/typing
        //_ = PushValidityAsync(ctx);
        //if (ctx is null) return;

        //_subscribedCtx = ctx;
        //ctx.OnValidationStateChanged += HandleValidationStateChanged;

        //// Optional: enable this if you want per-keystroke/per-field updates:
        //// editContext.OnFieldChanged += HandleFieldChanged;

        //// Push an initial snapshot now that we have a context
        //_ = PushValidityFromEditContextAsync(ctx);
    }
    private void RehydrateValidationUiIfNeeded(EditContext ctx)
    {
        // If registry remembers this component type as invalid, show messages now.
        var wasInvalid =
            PanelRegistry?.LastKnownValidityByType.TryGetValue(GetType(), out var lastKnown) == true
            && lastKnown == false;

        if (wasInvalid)
        {
            // This will mark fields as modified, run EC.Validate(), and (below) we'll push combined truth.
            _ = ValidateModel();
        }
    }

    private void Unsubscribe()
    {
        if (_subscribedCtx is null) return;
        _subscribedCtx.OnValidationStateChanged -= HandleValidationStateChanged;
        // _subscribedEditContext.OnFieldChanged -= HandleFieldChanged;
        _subscribedCtx = null;
    }
    private async Task PushValidityAsync(EditContext? ctx)
    {
        if (ctx is null) return;
        var isValid = !ctx.GetValidationMessages().Any();
        PanelRegistry?.SetValidity(this, isValid);
        await Task.CompletedTask;
    }

    private void HandleValidationStateChanged(object? sender, ValidationStateChangedEventArgs e)
        => _ = PushValidityFromEditContextAsync(sender as EditContext);

    // Optional if you want more frequent updates:
    // private void HandleFieldChanged(object? sender, FieldChangedEventArgs e)
    //     => _ = PushValidityFromEditContextAsync(sender as EditContext);

    private async Task PushValidityFromEditContextAsync(EditContext? ctx)
    {
        if (ctx is null)
        {
            Logger?.LogDebug("EditContext null; skipping validity push for {Panel}", GetType().Name);
            return;
        }
        var fvValid = Validator is null ? true : (await Validator.ValidateAsync(Model)).IsValid;

        // (Optional) combine with EC messages if you also use data annotations or other validators
        // var ecValid = !ctx.GetValidationMessages().Any();
        // var finalValid = fvValid && ecValid;

        var finalValid = fvValid;

        PanelRegistry?.SetValidity(this, finalValid);
        Logger?.LogInformation("PushValidityFromEditContextAsync({Panel}) -> IsValid: {IsValid}", GetType().Name, finalValid);
        await Task.CompletedTask;
    }
    /// <summary>
    /// Provides a reference to the model used by the derived panel.
    /// </summary>
    protected abstract TModel Model { get; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // Try to get an EditContext from @ref *after* render
        var ctx = editForm?.EditContext;

        if (ctx is not null && !ReferenceEquals(_subscribedCtx, ctx))
        {
            Unsubscribe();
            Subscribe(ctx);
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    protected virtual async Task<bool> ValidateModel()
    {
        var editContextValid = await ShowAllValidationErrors();
        if (Validator is null)
        {
            // Registry already updated by ShowAllValidationErrors
            Logger?.LogInformation("ValidateModel - Validator for {Name} is null...", this.GetType().Name);
            return editContextValid;
        }
        else
        {
            Logger?.LogInformation("ValidateModel - Using validator for {Name}", Validator.GetType().Name);
        }
        var fv = await Validator.ValidateAsync(Model);
        var finalValid = editContextValid && fv.IsValid;
        Logger?.LogInformation("ValidateModel - Validator result for {Name} is {Valid}...", this.GetType().Name, finalValid);

        // Update the registry with the combined truth
        PanelRegistry?.SetValidity(this, finalValid);

        return finalValid;
        //var result = await Validator.ValidateAsync(Model);
        //PanelRegistry?.SetValidity(this, result.IsValid); // <— report validity
        //return result.IsValid;
    }

    private async Task<bool> ShowAllValidationErrors()
    {
        var editContext = CurrentEditContext ?? editForm?.EditContext;
        if (editContext == null)
        {
            Logger?.LogWarning("ShowAllValidationErrors: EditContext is null for {Panel}", GetType().Name);

            // No form → treat as valid (or skip touching the registry)
            //PanelRegistry?.SetValidity(this, true);
            return true;
        }

        // Mark all properties as having been interacted with
        MarkAllFieldsAsModified(editContext);

        // Validate the form
        var isValid = editContext.Validate();
        // Optionally also run FV here (you already do inside ValidateModel)
        if (Validator is not null)
        {
            var fvValid = (await Validator.ValidateAsync(Model)).IsValid;
            Logger?.LogInformation("ShowAllValidationErrors: EC={ECValid}, FV={FVValid} for {Panel}",
                                   isValid, fvValid, GetType().Name);
            // Let ValidateModel() compute/push the final combined truth.
        }
        else
        {
            // If no FV, ensure registry reflects EC result immediately
            PanelRegistry?.SetValidity(this, isValid);
        }

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
        Logger?.LogInformation("CanCloseModalAsync - {Name} isValid: {Valid}", typeof(TModel).Name, isValid);
        return isValid;
    }

    // ✔ save-on-navigate (or on close)
    public virtual async Task SaveOnNavigateAsync()
    {
        // Here we'd normally map VM → Entity and mark dirty (via state action)
        // For now we simply validate and assume mapping will occur in save action.
        var isValid = await ValidateModel();
        Logger?.LogInformation("SaveOnNavigateAsync - {Name} isValid: {Valid}", typeof(TModel).Name, isValid);
    }
}
