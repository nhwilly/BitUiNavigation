using BitUiNavigation.Client.Pages.Modals;
using FluentValidation;

namespace BitUiNavigation.Client.Pages.UserProfile;
public record UserProfileViewModel : BaseRecord
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string UpdatedAt { get; set; } = string.Empty;
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
    public async Task<UserDto> SaveUserAsync(UserDto userDto)
    {
        Console.WriteLine("Saving user inside userService...");
        await Task.Delay(1000);
        return userDto;
    }

    public async Task<UserDto> GetUserAsync(string userId)
    {
        Console.WriteLine("Getting user inside userService...");
        await Task.Delay(1000);
        var u = new UserDto() { FirstName = "bill", LastName = "noel", UpdatedAt = DateTimeOffset.UtcNow };
        return await Task.FromResult(u);
    }

}