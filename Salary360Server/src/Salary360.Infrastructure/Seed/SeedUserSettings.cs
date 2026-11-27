namespace Salary360.Infrastructure.Seed;

public sealed class SeedUserSettings
{
    public string Email { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string DateOfBirth { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string ProfilePictureUrl { get; set; } = string.Empty;

    public SeedAddressSettings Address { get; set; } = new();
}
