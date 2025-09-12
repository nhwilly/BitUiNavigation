namespace BitUiNavigation.Client.Features.UserProfile.Provider;
public static class NavSectionExtensions
{
    public static void AddSectionIfItemsExist(this IList<NavSection> sections, NavSection navSection)
    {
        if (navSection.CustomNavItems.Count > 0)
            sections.Add(navSection);
    }
}
