using Microsoft.AspNetCore.Components.Forms;

namespace BitUiNavigation.Client.ModalHost.Abstract;

public abstract class ModalPanelBase<TModel> :
    TimeWarpStateComponent,
    IModalPanel
    where TModel : BaseRecord
{
    [Inject] public ILogger<TModel>? Logger { get; set; }

    [CascadingParameter] protected ModalContext? ModalContext { get; set; }

    [CascadingParameter] protected EditContext? CurrentEditContext { get; set; }

    protected EditForm? editForm;

    private EditContext? _editContext;
    private ModalHostState ModalHostState => GetState<ModalHostState>();

    // ---------------- lifecycle ----------------

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        var context = CurrentEditContext ?? editForm?.EditContext;

        if (!ReferenceEquals(_editContext, context))
        {
            Unsubscribe();
            SubscribeIfPresent(context);
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

    private void SubscribeIfPresent(EditContext? editContext)
    {
        if (editContext is null) return;

        _editContext = editContext;
        //_editContext.OnFieldChanged += OnFieldChanged;
        _editContext.OnValidationStateChanged += OnValidationStateChanged;

        RehydrateIfPreviouslyInvalid(_editContext);
        Logger?.LogDebug("Subscribed to EditContext for {Panel}", GetType().Name);
    }

    private void Unsubscribe()
    {
        if (_editContext is null) return;

        //_editContext.OnFieldChanged -= OnFieldChanged;
        _editContext.OnValidationStateChanged -= OnValidationStateChanged;
        Logger?.LogDebug("Unsubscribed from EditContext for {Panel}", GetType().Name);
        _editContext = null;
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
        var ctx = _editContext ?? CurrentEditContext ?? editForm?.EditContext;
        if (ctx is null || ModalContext is null)
        {
            Logger?.LogTrace("Publish skipped: ctx or Ctx null for {Panel}", GetType().Name);
            return;
        }

        var errorCount = ctx.GetValidationMessages().Count();
        var isValid = errorCount == 0;

        // TODO can we check and not invoke an update if exists and unchanged?
        await ModalHostState.SetValidity(
            ModalContext.ProviderName,
            ModalContext.PanelName,
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
        var editContext = _editContext ?? CurrentEditContext ?? editForm?.EditContext;
        if (editContext is null)
        {
            Logger?.LogWarning("ForceValidateAndPublishAsync: EditContext is null for {Panel}", GetType().Name);
            return true; // treat as valid if no form
        }

        MarkAllFieldsAsModified(editContext); // make visuals show immediately
        var isValid = editContext.Validate();      // triggers DA and FV validator components
        await PublishFromEditContextAsync();
        return isValid;
    }

    private void RehydrateIfPreviouslyInvalid(EditContext editContext)
    {
        try
        {
            if (ModalContext is null) return;

            // Reads central state: Provider → Panel → PanelValidity
            if (ModalHostState.Validity.TryGetValue(ModalContext.ProviderName, out var panelName) &&
                panelName.TryGetValue(ModalContext.PanelName, out var panelValidity) &&
                panelValidity.IsValid == false)
            {
                // Show messages immediately to match last-known state
                MarkAllFieldsAsModified(editContext);
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

    public virtual async Task<bool> CanCloseModalAsync()
    {
        var canClose = await ForceValidateAndPublishAsync();
        Logger?.LogDebug("CanCloseModalAsync → {Valid} for {Panel}", canClose, GetType().Name);
        return canClose;
    }
}
