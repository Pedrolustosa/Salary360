using Salary360.Application.Auth;
using Salary360.Application.Auth.Dtos;

namespace Salary360.API.Contracts.Requests.Auth;

public sealed record UpdateProfileRequest(
    string DisplayName,
    DateOnly DateOfBirth,
    string? PhoneNumber,
    string? ProfilePictureUrl,
    UserAddressDto? Address);
