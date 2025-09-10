namespace BitUiNavigation.Client.Features.UserProfile.Memberships;

public class UserMembershipsViewModelValidator : AbstractValidator<UserMembershipsViewModel>
{
    public UserMembershipsViewModelValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name be required");

    }
}
