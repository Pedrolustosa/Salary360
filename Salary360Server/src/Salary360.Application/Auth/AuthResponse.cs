namespace Salary360.Application.Auth;

public sealed record AuthResponse(
    Guid UserId,
    string Email,
    string DisplayName,
    DateOnly? DateOfBirth,
    string? ProfilePictureUrl,
    string AccessToken,
    DateTime ExpiresAtUtc,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions);
