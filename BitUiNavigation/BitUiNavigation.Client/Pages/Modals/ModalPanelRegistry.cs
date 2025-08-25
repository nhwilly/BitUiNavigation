namespace BitUiNavigation.Client.Pages.Modals;

internal sealed class ModalPanelRegistry : IModalPanelRegistry
{
    private readonly List<IModalPanel> _live = [];
     public IReadOnlyList<IModalPanel> LivePanels => _live;

   private readonly Dictionary<Type, bool> _lastKnownByType  = [];
    public IReadOnlyDictionary<Type, bool> LastKnownValidityByType  => _lastKnownByType ;


    public void Register(IModalPanel panel)
    {
        if (!_live.Contains(panel)) _live.Add(panel);
    }

    public void Unregister(IModalPanel panel)
    {
        _live.Remove(panel);
        //_validity.Remove(panel); // drop stale validity for disposed instances
    }
    public void SetValidity(IModalPanel panel, bool isValid)
    {
        var type = panel.GetType();
        _lastKnownByType[type] = isValid;
    }
}
