namespace BitUiNavigation.Client.Pages.Modal.Providers;

public class NavSectionDetail
{
    public string IconName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public bool HasHeader => !string.IsNullOrEmpty(Title);
    public List<CustomNavItem> CustomNavItems { get; set; } = [];
}
