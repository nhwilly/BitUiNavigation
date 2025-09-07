using BitUiNavigation.Client.Pages.UserProfile.Memberships;

namespace BitUiNavigation.Client.Pages.UserProfile.Sometimes;

public class SometimesViewModelValidator : AbstractValidator<SometimesViewModel>
{
    public SometimesViewModelValidator()
    {
        RuleFor(x => x.Description).NotEmpty().WithMessage("Can we please have a description?");

    }
}
