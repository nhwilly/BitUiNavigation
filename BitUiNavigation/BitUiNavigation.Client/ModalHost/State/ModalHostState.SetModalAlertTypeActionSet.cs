namespace BitUiNavigation.Client.ModalHost.State;

public sealed partial class ModalHostState
{
    public static class SetModalAlertTypeActionSet
    {
        public sealed class Action : IAction
        {
            public ModalAlertType AlertType { get; }
            public string AlertMessage { get; init; } = string.Empty;
            public Action(ModalAlertType alertType, string? message = null)
            {
                AlertType = alertType;
                AlertMessage = message ?? string.Empty;
            }
        }
        public sealed class Handler : ActionHandler<Action>
        {
            private readonly ILogger<ModalHostState> _logger;
            private ModalHostState State => Store.GetState<ModalHostState>();
            public Handler(IStore store, ILogger<ModalHostState> logger) : base(store)
            {
                _logger = logger;
            }
            public override async Task Handle(Action action, CancellationToken cancellationToken)
            {
                _logger.LogDebug("SetModalAlertType Type={AlertType} Message={Message}", action.AlertType, action.AlertMessage);
                State.ModalAlertType = action.AlertType;
                State.ModalAlertMessage = action.AlertMessage;
                await Task.CompletedTask;
            }
        }
    }

}
