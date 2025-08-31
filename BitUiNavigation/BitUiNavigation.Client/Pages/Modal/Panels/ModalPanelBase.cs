using System.Linq;
using BitUiNavigation.Client.Pages.Modal.Abstract;
using BitUiNavigation.Client.Services;
using FluentValidation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeWarp.State;

namespace BitUiNavigation.Client.Pages.Modal.Panels;

public abstract class ModalPanelBase<TModel> :
    TimeWarpStateComponent,
    IModalPanel
    where TModel : BaseRecord
{
    [Inject] public ILogger<TModel>? Logger { get; set; }

    // Optional: present if you also place <FluentValidationValidator /> in the form.
    // We don't call it directly for counting; EditContext already aggregates messages.
    [Inject] private IValidator<TModel>? Validator { get; set; } = default!;

    // Provided by ModalHost (which panel/provider this is)
    [CascadingParameter] protected ModalContext? Ctx { get; set; }

    // Non-null only if this component is inside the EditForm (rare).
    [CascadingParameter] protected EditContext? CurrentEditContext { get; set; }

    // If the form lives in the derived .razor, set @ref="editForm" there.
    protected EditForm? editForm;
    private EditContext? _ctx; // the subscribed context

    private ModalHostState ModalHostState => GetState<ModalHostState>();

    // ---------------- lifecycle ----------------

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // Prefer cascaded EditContext; else fall back to @ref’d form
        var discovered = CurrentEditContext ?? editForm?.EditContext;

        if (!ReferenceEquals(_ctx, discovered))
        {
            Unsubscribe();
            SubscribeIfPresent(discovered);
            // Publish once so nav badges reflect current validity
            await PublishFromEditContextAsync();
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    public override void Dispose()
    {
        Unsubscribe();
        base.Dispose();
    }

    // --------------- subscription ---------------

    private void SubscribeIfPresent(EditContext? ctx)
    {
        if (ctx is null) return;

        _ctx = ctx;
        _ctx.OnFieldChanged += OnFieldChanged;
        _ctx.OnValidationStateChanged += OnValidationStateChanged;

        RehydrateIfPreviouslyInvalid(_ctx);
        Logger?.LogDebug("Subscribed to EditContext for {Panel}", GetType().Name);
    }

    private void Unsubscribe()
    {
        if (_ctx is null) return;

        _ctx.OnFieldChanged -= OnFieldChanged;
        _ctx.OnValidationStateChanged -= OnValidationStateChanged;
        Logger?.LogDebug("Unsubscribed from EditContext for {Panel}", GetType().Name);
        _ctx = null;
    }

    private void OnFieldChanged(object? sender, FieldChangedEventArgs e)
    {
        Logger?.LogTrace("FieldChanged: {Field} in {Panel}", e.FieldIdentifier.FieldName, GetType().Name);
        _ = PublishFromEditContextAsync();
    }

    private void OnValidationStateChanged(object? sender, ValidationStateChangedEventArgs e)
    {
        Logger?.LogTrace("ValidationStateChanged in {Panel}", GetType().Name);
        _ = PublishFromEditContextAsync();
    }

    // --------------- validity -------------------

    private async Task PublishFromEditContextAsync()
    {
        var ctx = _ctx ?? CurrentEditContext ?? editForm?.EditContext;
        if (ctx is null || Ctx is null)
        {
            Logger?.LogTrace("Publish skipped: ctx or Ctx null for {Panel}", GetType().Name);
            return;
        }

        // What the UI actually shows (DA and/or FV messages)
        var errorCount = ctx.GetValidationMessages().Count();
        var isValid = errorCount == 0;

        await ModalHostState.SetValidity(
            Ctx.ProviderKey,
            Ctx.PanelName,
            isValid,
            errorCount,
            CancellationToken);

        Logger?.LogDebug("Publish validity for {Panel}: valid={Valid}, errors={Count}", GetType().Name, isValid, errorCount);
    }

    /// <summary>
    /// Force a full validation pass and publish result.
    /// Works with DataAnnotations and/or FluentValidation (via EditContext).
    /// </summary>
    protected async Task<bool> ForceValidateAndPublishAsync()
    {
        var ctx = _ctx ?? CurrentEditContext ?? editForm?.EditContext;
        if (ctx is null)
        {
            Logger?.LogWarning("ForceValidateAndPublishAsync: EditContext is null for {Panel}", GetType().Name);
            return true; // treat as valid if no form
        }

        MarkAllFieldsAsModified(ctx); // make visuals show immediately
        var ok = ctx.Validate();      // triggers DA and FV validator components
        await PublishFromEditContextAsync();
        return ok;
    }

    private void RehydrateIfPreviouslyInvalid(EditContext ctx)
    {
        try
        {
            if (Ctx is null) return;

            // Reads central state: Provider → Panel → PanelValidity
            if (ModalHostState.Validity.TryGetValue(Ctx.ProviderKey, out var perPanel) &&
                perPanel.TryGetValue(Ctx.PanelName, out var pv) &&
                pv.IsValid == false)
            {
                // Show messages immediately to match last-known state
                MarkAllFieldsAsModified(ctx);
            }
        }
        catch (Exception ex)
        {
            Logger?.LogDebug(ex, "RehydrateIfPreviouslyInvalid failed for {Panel}", GetType().Name);
        }
    }

    private static void MarkAllFieldsAsModified(EditContext editContext)
    {
        var props = editContext.Model.GetType().GetProperties()
            .Where(p => p.CanRead && p.CanWrite);

        foreach (var p in props)
        {
            var id = new FieldIdentifier(editContext.Model, p.Name);
            editContext.NotifyFieldChanged(id);
        }
    }

    // ------------- abstract surface -------------

    /// <summary>The model used by the derived panel.</summary>
    protected abstract TModel Model { get; }

    // ------------- IModalPanel / ISupportsSaveOnNavigate -------------

    public virtual Task<bool> CanNavigateToAnotherSectionAsync() => Task.FromResult(true);

    public virtual async Task<bool> CanCloseModalAsync()
    {
        var ok = await ForceValidateAndPublishAsync();
        Logger?.LogDebug("CanCloseModalAsync → {Valid} for {Panel}", ok, GetType().Name);
        return ok;
    }

    public virtual async Task SaveOnNavigateAsync()
    {
        var ok = await ForceValidateAndPublishAsync();
        Logger?.LogDebug("SaveOnNavigateAsync → {Valid} for {Panel}", ok, GetType().Name);
        // map VM → entity & dispatch save here if desired
    }
}
