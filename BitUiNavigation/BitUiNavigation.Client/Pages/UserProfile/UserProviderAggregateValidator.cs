namespace BitUiNavigation.Client.Pages.UserProfile;

using FluentValidation;

public sealed class UserProviderAggregateValidator : AbstractValidator<UserProviderAggregate>
{
    public UserProviderAggregateValidator()
    {
        //RuleFor(x => x.AccountId)
        //    .NotEmpty().WithMessage("An account is required.");

        //RuleFor(x => x.LocationId)
        //    .NotEmpty().WithMessage("A location is required.");

        // Add any cross-panel / cross-state rules here
        // e.g. When(...).Must(...).WithMessage(...)
    }
}
