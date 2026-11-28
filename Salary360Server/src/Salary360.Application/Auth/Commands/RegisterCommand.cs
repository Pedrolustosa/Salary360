using Salary360.Application.Interfaces.Messaging;
using Salary360.Application.Auth.Dtos;

namespace Salary360.Application.Auth.Commands;

public sealed record RegisterCommand(
    string Email,
    string Password,
    string DisplayName,
    DateOnly DateOfBirth,
    string? PhoneNumber,
    bool AcceptedTerms,
    bool AcceptedPrivacyPolicy) : ICommand<UserProfileDto>;
