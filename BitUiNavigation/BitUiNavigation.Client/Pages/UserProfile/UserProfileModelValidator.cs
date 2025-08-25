using FluentValidation;

namespace BitUiNavigation.Client.Pages.UserProfile;

public class UserProfileModelValidator : AbstractValidator<UserProfileViewModel>
{
    public UserProfileModelValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().WithMessage("First name is required");
        RuleFor(x => x.LastName).NotEmpty().WithMessage("Last name is required");
    }
}
public class UserMembershipsViewModelValidator : AbstractValidator<UserMembershipsViewModel>
{
    public UserMembershipsViewModelValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name be required");

    }
}
