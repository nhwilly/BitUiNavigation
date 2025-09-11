namespace BitUiNavigation.Client.Pages.ModalHost.State;

public sealed partial class ModalHostState : State<ModalHostState>
{
    public override void Initialize()
    {
        _validity?.Clear();
        ModalAlertType = ModalAlertType.None;
        NavSections = [];
        Saving = false;
    }
    private readonly Dictionary<string, Dictionary<string, PanelValidity>> _validity = [];
    public IReadOnlyDictionary<string, Dictionary<string, PanelValidity>> Validity => _validity;
    public bool Saving { get; private set; } = false;
    public ModalAlertType ModalAlertType { get; private set; } = ModalAlertType.None;
    public string ModalAlertMessage { get; private set; } = string.Empty;
    public List<NavSectionDetail> NavSections { get; private set; } = [];
    public string Title { get; private set; } = string.Empty;

    /// <summary>
    /// True if every expected panel for the provider is valid.
    /// If a panel hasn't published yet, it's treated as valid unless missingBlocks==true.
    /// </summary>
    public bool ArePanelsValid(string providerKey,
                               IEnumerable<string> expectedPanelKeys,
                               bool missingBlocks = false)
    {
        if (!Validity.TryGetValue(providerKey, out var perPanel))
            return !missingBlocks; // nothing published yet

        foreach (var key in expectedPanelKeys)
        {
            if (!perPanel.TryGetValue(key, out var pv))
            {
                if (missingBlocks) return false;
                continue;
            }
            if (!pv.IsValid) return false;
        }
        return true;
    }

}
