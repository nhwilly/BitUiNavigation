namespace BitUiNavigation.Client.Pages.UserProfile.Provider;

public sealed record UserProviderAggregate(
Guid AccountId,
Guid LocationId
// add any other cross-panel facts you need here
);
