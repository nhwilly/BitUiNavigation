namespace BitUiNavigation.Client.Features.UserProfile;

public record UserDto
{
    public required string FirstName { get; set; }
    public required string Name { get; set; }
    public required string LastName { get; set; }
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
