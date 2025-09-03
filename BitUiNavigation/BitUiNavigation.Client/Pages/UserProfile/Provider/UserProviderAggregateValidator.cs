namespace BitUiNavigation.Client.Pages.UserProfile.Provider;

using FluentValidation;

public sealed class UserProviderAggregateValidator : AbstractValidator<UserProviderAggregate>
{
    public UserProviderAggregateValidator()
    {
        //RuleFor(x => x.AccountId)
        //    .NotEmpty().WithMessage("An account is required.");

        //RuleFor(x => x.LocationId)
        //    .NotEmpty().WithMessage("A location is required.");

        //RuleFor(x => x).Custom((aggregate, context) =>
        //{
        //    context.AddFailure("This is a general error not tied to any property.");
        //    context.AddFailure("AccountId", "This is an error tied to the AccountId property.");
        //    context.AddFailure("LocationId", "This is an error tied to the LocationId property.");  
        //});
        //Add any cross - panel / cross - state rules here
        // e.g.When(...).Must(...).WithMessage(...)
    }
}
