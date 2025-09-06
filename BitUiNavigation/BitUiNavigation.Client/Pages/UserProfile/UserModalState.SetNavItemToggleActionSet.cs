namespace BitUiNavigation.Client.Pages.UserProfile;
public sealed partial class UserModalState
{
    public static class SetNavItemToggleActionSet
    {
        public sealed class Action : IAction
        {
            public bool IsToggled { get; }
            public Action(bool isToggled)
            {
                IsToggled = isToggled;
            }
        }
        public sealed class Handler : ActionHandler<Action>
        {
            private readonly ILogger<UserModalState> _logger;
            private readonly IModalProvider _provider;
            private readonly NavigationManager _nav;
            private UserModalState State => Store.GetState<UserModalState>();
            public Handler(
                IStore store,
                IServiceProvider serviceProvider,
                ILogger<UserModalState> logger,
                NavigationManager nav)
                : base(store)
            {
                _provider = serviceProvider.GetRequiredKeyedService<IModalProvider>("User");
                _logger = logger;
                _nav = nav;
            }
            private UserModalState UserModalState => Store.GetState<UserModalState>();
            public override async Task Handle(Action action, CancellationToken cancellationToken)
            {
                UserModalState.IsToggled = !UserModalState.IsToggled;
                await _provider.BuildNavSections(_nav, cancellationToken);
            }
        }
    }
}
