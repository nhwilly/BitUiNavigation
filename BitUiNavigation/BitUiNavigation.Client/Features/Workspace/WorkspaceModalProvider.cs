//using Bit.BlazorUI;
//using BitUiNavigation.Client.Pages.Modals;
//using BitUiNavigation.Client.Pages.UserProfile;
//using BitUiNavigation.Client.Services;
//using Microsoft.AspNetCore.Components;
//using TimeWarp.State;

//namespace BitUiNavigation.Client.Pages.Workspace;

//public class WorkspaceModalProvider : ModalProviderBase
//{
//    public override string ProviderName => "Workspace";

//    public override string DefaultPanel => nameof(UserMembershipsPanel);

//    public override string Width => "900px";
//    public override string Height => "640px";
//    private UserModalState State => Store.GetState<UserModalState>();
//    private ModalHostState ModalHostState => Store.GetState<ModalHostState>();
//    [Parameter, SupplyParameterFromQuery] public Guid AccountId { get; set; }
//    [Parameter, SupplyParameterFromQuery] public Guid LocationId { get; set; }

//    protected override Dictionary<string, Type> PanelMap { get; } = new(StringComparer.OrdinalIgnoreCase)
//    {
//        [nameof(UserMembershipsPanel)] = typeof(UserMembershipsPanel),
//        [nameof(UserProfilePanel)] = typeof(UserProfilePanel)
//    };

//    public WorkspaceModalProvider(IStore store, ILogger<WorkspaceModalProvider> logger) : base(store, logger) { }

//    public override List<NavSectionDetail> BuildCustomNavSections(NavigationManager nav)
//    {
//        var sections = new List<NavSectionDetail>();
//        sections.Add(new NavSectionDetail()
//        {
//            Title = "Settings",
//            IconName = BitIconName.Settings,
//            CustomNavItems =
//                [
//                    new() { Key = nameof(UserMembershipsPanel), Text = "Memberships", IconName = BitIconName.UserEvent, Url = BuildPanelUrl(nav, nameof(UserMembershipsPanel)) },
//                    new() { Key = nameof(UserProfilePanel), Text = "Profile", IconName = BitIconName.Contact, Url = BuildPanelUrl(nav, nameof(UserProfilePanel)) }
//                ]
//        });

//        foreach (var section in sections)
//        {
//            DecorateCustomNavItemsWithValidationIndicators(section.CustomNavItems);
//        }
//        return sections;
//    }
//    public override async Task OnModalOpeningAsync(CancellationToken ct)
//    {
//        await State.SetIsLoading(true, ct);
//        await Task.CompletedTask;
//    }
//    public override async Task OnModalOpenedAsync(CancellationToken ct)
//    {
//        await State.BeginUserEditSession(AccountId, LocationId, ct);
//        await ModalHostState.SetTitle(State.ProviderTitle, ct);
//        await State.SetIsLoading(false, ct);
//    }

//}
