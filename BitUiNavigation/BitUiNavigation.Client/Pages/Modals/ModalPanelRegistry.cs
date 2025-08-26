namespace BitUiNavigation.Client.Pages.Modals;

internal sealed class ModalPanelRegistry : IModalPanelRegistry
{
    private readonly ILogger<ModalPanelRegistry> _logger;

    public ModalPanelRegistry(ILogger<ModalPanelRegistry> logger)
    {
        _logger = logger;
    }

    private readonly List<IModalPanel> _live = [];
    public IReadOnlyList<IModalPanel> LivePanels => _live;

    private readonly Dictionary<Type, bool> _lastKnownByType = [];
    public IReadOnlyDictionary<Type, bool> LastKnownValidityByType => _lastKnownByType;


    public void Register(IModalPanel panel)
    {
        var panelExists = _live.Contains(panel);
        if (!panelExists)
            _live.Add(panel);
        _logger.LogInformation("Registered panel {PanelType}, total live panels: {Count}", panel.GetType().Name, _live.Count);
    }

    public void Unregister(IModalPanel panel)
    {
        var panelExists = _live.Contains(panel);
        if (panelExists)
            _live.Remove(panel);

        _logger.LogInformation("Unregistered panel {PanelType}, total live panels: {Count}", panel.GetType().Name, _live.Count);
    }
    public void SetValidity(IModalPanel panel, bool isValid)
    {
        var panelExists = _live.Contains(panel);
        var type = panel.GetType();
        _logger.LogInformation("Setting validity for panel {PanelType} to {IsValid}, panel exists: {PanelExists} lastKnownByType", type.Name, isValid, panelExists);

        var lastKnown = _lastKnownByType.TryGetValue(type, out bool value) ? value : (bool?)null;
        _logger.LogInformation("Previous known validity for panel {PanelType} was {LastKnown}", type.Name, lastKnown.HasValue ? lastKnown.Value.ToString() : "null");
        _lastKnownByType[type] = isValid;

    }
}
