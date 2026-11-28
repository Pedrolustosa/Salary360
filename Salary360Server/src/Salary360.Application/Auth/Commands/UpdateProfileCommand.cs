using Salary360.Application.Interfaces.Messaging;
using Salary360.Application.Auth.Dtos;

namespace Salary360.Application.Auth.Commands;

public sealed record UpdateProfileCommand(
    string DisplayName,
    DateOnly DateOfBirth,
    string? PhoneNumber,
    string? ProfilePictureUrl,
    UserAddressDto? Address) : ICommand<UserProfileDto>;
