namespace BitUiNavigation.Client.ModalHost.Navigation;

/// <summary>
/// Contains the title of the section and all the nav items within it.
/// </summary>
public class NavSection
{
    public string IconName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public bool HasHeader => !string.IsNullOrEmpty(Title);
    public List<CustomNavItem> CustomNavItems { get; set; } = [];
}
