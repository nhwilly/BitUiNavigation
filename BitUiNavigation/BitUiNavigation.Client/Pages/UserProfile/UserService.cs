namespace BitUiNavigation.Client.Pages.UserProfile;

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
        await Task.Delay(250);
        var u = new UserDto() {Name="bob", FirstName = "Wondky", LastName = "Dude", UpdatedAt = DateTimeOffset.UtcNow };
        return await Task.FromResult(u);
    }

}