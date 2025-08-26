using FluentValidation;

namespace BitUiNavigation.Client.Pages.UserProfile;

public class UserMembershipsViewModelValidator : AbstractValidator<UserMembershipsViewModel>
{
    public UserMembershipsViewModelValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name be required");

    }
}
