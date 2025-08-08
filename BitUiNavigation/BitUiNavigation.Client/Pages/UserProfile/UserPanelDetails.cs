using BitUiNavigation.Client.Pages.Modals;
using FluentValidation;

namespace BitUiNavigation.Client.Pages.UserProfile;
public record UserProfileViewModel : BaseRecord
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}
public class UserProfileModelValidator : AbstractValidator<UserProfileViewModel>
{
    public UserProfileModelValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().WithMessage("First name is required");
        RuleFor(x => x.LastName).NotEmpty().WithMessage("Last name is required");
    }
}

public class UserService
{
    public async Task SaveUserAsync(UserProfileViewModel model)
    {
        Console.WriteLine("Saving user inside userService...");
        await Task.Delay(5000);
    }

    public async Task<UserProfileViewModel> GetUserAsync(Guid accountId, Guid locationId)
    {
        Console.WriteLine("Getting user inside userService...");
        await Task.Delay(10000);
        var p = new UserProfileViewModel() { FirstName = "bill", LastName = "noel" };
        return await Task.FromResult(p);
    }

}