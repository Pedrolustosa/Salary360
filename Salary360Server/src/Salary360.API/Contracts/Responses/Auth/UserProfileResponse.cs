using Salary360.Domain.Enums;

namespace Salary360.API.Contracts.Responses.Auth;

public sealed record UserProfileResponse(
    Guid UserId,
    string Email,
    string DisplayName,
    DateOnly? DateOfBirth,
    string? PhoneNumber,
    UserStatus Status,
    DateTimeOffset? TermsAcceptedAt,
    DateTimeOffset? PrivacyPolicyAcceptedAt,
    string? ProfilePictureUrl,
    UserAddressResponse? Address,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    IReadOnlyList<string> Roles);
