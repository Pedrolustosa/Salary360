using Salary360.Application.Auth;
using Salary360.Domain.Enums;

namespace Salary360.Application.Interfaces.Identity;

public interface IIdentityService
{
    Task<UserProfileDto> RegisterAsync(
        string email,
        string password,
        string displayName,
        DateOnly dateOfBirth,
        string? phoneNumber,
        DateTimeOffset termsAcceptedAt,
        DateTimeOffset privacyPolicyAcceptedAt,
        CancellationToken cancellationToken = default);

    Task<AuthResponse> LoginAsync(string email, string password, CancellationToken cancellationToken = default);

    Task ConfirmEmailAsync(Guid userId, string token, CancellationToken cancellationToken = default);

    Task ResendConfirmationAsync(string email, CancellationToken cancellationToken = default);

    Task<UserProfileDto> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<UserProfileDto> UpdateProfileAsync(
        Guid userId,
        string displayName,
        DateOnly dateOfBirth,
        string? phoneNumber,
        string? profilePictureUrl,
        UserAddressDto? address,
        CancellationToken cancellationToken = default);

    Task<UserProfileDto> SetStatusAsync(Guid userId, UserStatus status, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserProfileDto>> ListUsersAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RoleDto>> ListRolesAsync(CancellationToken cancellationToken = default);

    Task<RoleDto> UpdateRolePermissionsAsync(
        string roleName,
        IReadOnlyList<string> permissions,
        CancellationToken cancellationToken = default);
}
