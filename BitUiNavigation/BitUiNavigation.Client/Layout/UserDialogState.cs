using TimeWarp.State;

namespace BitUiNavigation.Client.Layout;

public sealed partial class UserDialogState : State<UserDialogState>
{
    public override void Initialize() { }
    public string CurrentPage { get; private set; } = string.Empty;

    public static class SetPageActionSet
    {
        public sealed class Action : IAction
        {
            public string Page { get; }
            public Action(string page)
            {
                Page = page;
            }
        }
        public sealed class Handler : ActionHandler<Action>
        {
            public Handler(IStore store) : base(store) { }

            private UserDialogState UserDialogState => Store.GetState<UserDialogState>();
            public override async Task Handle(Action action, CancellationToken cancellationToken)
            {
                UserDialogState.CurrentPage = action.Page;
                await Task.CompletedTask;
            }
        }
    }
}