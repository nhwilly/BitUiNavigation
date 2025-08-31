namespace BitUiNavigation.Client.Pages.Modal;

public class CustomNavItem
{
    public string Key { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string? Title { get; set; } = string.Empty;
    public string? AriaLabel { get; set; } = string.Empty;

    public string? IconName { get; set; } = string.Empty;
    public string? ValidationIconName { get; set; } = string.Empty;
    public string? Url { get; set; } = string.Empty;
    public bool IsExpanded { get; set; }
    public List<CustomNavItem> Children { get; set; } = [];
}
