using FluentValidation;

namespace BitUiNavigation.Client.Pages.UserProfile;
public record UserProfileViewModel
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
        // Simulate saving user data
        await Task.CompletedTask;
    }

    public async Task GetUserAsync(Guid accountId, Guid locationId)
    {
        Console.WriteLine("Getting user inside userService...");
        // Simulate saving user data
        await Task.CompletedTask;
    }

}