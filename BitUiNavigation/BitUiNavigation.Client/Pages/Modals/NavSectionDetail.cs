namespace BitUiNavigation.Client.Pages.Modals;

public class NavSectionDetail
{
    public string IconName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public bool HasHeader => !string.IsNullOrEmpty(Title);
    public List<CustomNavItem> CustomNavItems { get; set; } = [];
}
