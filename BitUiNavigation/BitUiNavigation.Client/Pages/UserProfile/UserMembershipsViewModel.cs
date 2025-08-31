using BitUiNavigation.Client.Pages.Modal.Abstract;

namespace BitUiNavigation.Client.Pages.UserProfile;

/// <summary>
/// View model used by the "Memberships" panel inside the User Modal.
/// You can expand this with whatever child collections or flags you need.
/// </summary>
public record UserMembershipsViewModel : BaseRecord
{
    // A list of child items being edited in this panel.
    //public List<MembershipItemViewModel> Items { get; init; } = new();

    // Optional: track the selected item in the list
    //public Guid? SelectedItemId { get; set; }
    public string Name { get; set; } = string.Empty;
    // Optional: working/viewmodel for the selected item
    //public MembershipItemViewModel? SelectedItemVm { get; set; }
}

/// <summary>
/// Represents one membership row in the list.
/// </summary>
public record MembershipItemViewModel
{
    public Guid Id { get; init; }
    public string RoleName { get; set; } = string.Empty;
    public DateTimeOffset? ExpirationDate { get; set; }
}
