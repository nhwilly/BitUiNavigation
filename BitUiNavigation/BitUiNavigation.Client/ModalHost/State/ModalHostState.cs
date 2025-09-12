namespace BitUiNavigation.Client.ModalHost.State;

public sealed partial class ModalHostState : State<ModalHostState>
{
    public override void Initialize()
    {
        _validity?.Clear();
        ModalAlertType = ModalAlertType.None;
        NavSections = [];
        IsBusy = false;
    }
    private readonly Dictionary<string, Dictionary<string, PanelValidity>> _validity = [];
    public IReadOnlyDictionary<string, Dictionary<string, PanelValidity>> Validity => _validity;
    public bool IsBusy { get; private set; } = false;
    public ModalAlertType ModalAlertType { get; private set; } = ModalAlertType.None;
    public string ModalAlertMessage { get; private set; } = string.Empty;
    public List<NavSection> NavSections { get; private set; } = [];

    /// <summary>
    /// True if every expected panel for the provider is valid.
    /// If a panel hasn't published yet, it's treated as valid unless validateMissingPanels==true.
    /// </summary>
    public bool ArePanelsValid(string providerName,
                               IEnumerable<string> panelNames,
                               bool validateMissingPanels = false)
    {
        if (!Validity.TryGetValue(providerName, out var providerValidity))
            return !validateMissingPanels; // nothing published yet

        foreach (var panelName in panelNames)
        {
            if (!providerValidity.TryGetValue(panelName, out var panelValidity))
            {
                if (validateMissingPanels) return false;
                continue;
            }
            if (!panelValidity.IsValid) return false;
        }
        return true;
    }

}
