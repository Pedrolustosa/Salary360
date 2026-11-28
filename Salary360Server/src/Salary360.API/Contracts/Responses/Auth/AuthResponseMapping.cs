using Salary360.Application.Auth.Dtos;
using AppAuthResponse = Salary360.Application.Auth.Dtos.AuthResponse;

namespace Salary360.API.Contracts.Responses.Auth;

public static class AuthResponseMapping
{
    public static UserAddressResponse ToResponse(this UserAddressDto dto)
        => new(
            dto.PostalCode,
            dto.Street,
            dto.Number,
            dto.Complement,
            dto.Neighborhood,
            dto.City,
            dto.State,
            dto.Country);

    public static RoleResponse ToResponse(this RoleDto dto)
        => new(dto.Name, dto.Permissions);

    public static IReadOnlyList<RoleResponse> ToResponse(this IReadOnlyList<RoleDto> dtos)
        => dtos.Select(x => x.ToResponse()).ToList();

    public static UserProfileResponse ToResponse(this UserProfileDto dto)
        => new(
            dto.UserId,
            dto.Email,
            dto.DisplayName,
            dto.DateOfBirth,
            dto.PhoneNumber,
            dto.Status,
            dto.TermsAcceptedAt,
            dto.PrivacyPolicyAcceptedAt,
            dto.ProfilePictureUrl,
            dto.Address?.ToResponse(),
            dto.CreatedAtUtc,
            dto.UpdatedAtUtc,
            dto.Roles);

    public static IReadOnlyList<UserProfileResponse> ToResponse(this IReadOnlyList<UserProfileDto> dtos)
        => dtos.Select(x => x.ToResponse()).ToList();

    public static AuthResponse ToResponse(this AppAuthResponse dto)
        => new(
            dto.UserId,
            dto.Email,
            dto.DisplayName,
            dto.DateOfBirth,
            dto.ProfilePictureUrl,
            dto.AccessToken,
            dto.ExpiresAtUtc,
            dto.Roles,
            dto.Permissions);
}
