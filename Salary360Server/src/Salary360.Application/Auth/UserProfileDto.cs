using Salary360.Domain.Enums;

namespace Salary360.Application.Auth;

public sealed record UserProfileDto(
    Guid UserId,
    string Email,
    string DisplayName,
    DateOnly? DateOfBirth,
    string? PhoneNumber,
    UserStatus Status,
    DateTimeOffset? TermsAcceptedAt,
    DateTimeOffset? PrivacyPolicyAcceptedAt,
    string? ProfilePictureUrl,
    UserAddressDto? Address,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    IReadOnlyList<string> Roles);
