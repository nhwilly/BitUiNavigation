namespace BitUiNavigation.Client.Pages.Modals;

public interface IModalPanelRegistry
{
    //void Register(IModalPanel panel);
    //void Unregister(IModalPanel panel);
    //// record the most recent validation result from this panel
    void SetValidity(IModalPanel panel, bool isValid);

    //IReadOnlyList<IModalPanel> LivePanels { get; }
    // expose a read-only view if the provider wants it
    IReadOnlyDictionary<Type, bool> LastKnownValidityByType { get; }

}
