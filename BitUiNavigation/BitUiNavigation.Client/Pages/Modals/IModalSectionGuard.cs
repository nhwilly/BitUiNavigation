namespace BitUiNavigation.Client.Pages.Modals;
/// <summary>
/// Allows a <see cref="ModalHost"/> to determine if a child <see cref="ModalPanelBase{TModel}"/> modal 
/// panel supports checking before closing the <see cref="ModalHost"/>
/// </summary>
public interface IModalPanel
{
    /// <summary>
    /// Allows a <see cref="ModalPanelBase{TModel}"/> to implement a check to 
    /// determine if the parent <see cref="ModalHost"/> can be closed.
    /// </summary>
    /// <returns></returns>
    Task<bool> CanCloseModalAsync();
}
