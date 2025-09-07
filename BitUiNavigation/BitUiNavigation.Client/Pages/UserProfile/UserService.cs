namespace BitUiNavigation.Client.Pages.UserProfile;

public class UserService
{
    public async Task<UserDto> SaveUserAsync(UserDto userDto, CancellationToken ct)
    {
        Console.WriteLine("Saving user inside userService...");
        await Task.Delay(5000, ct);
        return await Task.FromResult(userDto);

    }

    public async Task<UserDto> GetUserAsync(string userId, CancellationToken ct)
    {
        Console.WriteLine("Getting user inside userService...");
        await Task.Delay(5000, ct);
        var u = new UserDto() { Name = "bob", FirstName = "Wonky", LastName = "Dude", UpdatedAt = DateTimeOffset.UtcNow };
        return await Task.FromResult(u);
    }

}