using FluentValidation;
using Salary360.Application.Interfaces.Identity;
using Salary360.Application.Interfaces.Messaging;

namespace Salary360.Application.Auth.Commands;

public sealed record RegisterCommand(
    string Email,
    string Password,
    string DisplayName,
    DateOnly DateOfBirth,
    string? PhoneNumber,
    bool AcceptedTerms,
    bool AcceptedPrivacyPolicy) : ICommand<UserProfileDto>;

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email).ValidEmail();
        RuleFor(x => x.Password).StrongPassword();
        RuleFor(x => x.DisplayName).ValidDisplayName();
        RuleFor(x => x.DateOfBirth).ValidDateOfBirth();
        RuleFor(x => x.PhoneNumber).ValidOptionalPhone();
        RuleFor(x => x.AcceptedTerms).Equal(true);
        RuleFor(x => x.AcceptedPrivacyPolicy).Equal(true);
    }
}

public sealed class RegisterCommandHandler(IIdentityService identityService)
    : ICommandHandler<RegisterCommand, UserProfileDto>
{
    public Task<UserProfileDto> HandleAsync(RegisterCommand request, CancellationToken cancellationToken = default)
    {
        var acceptedAt = DateTimeOffset.UtcNow;
        return identityService.RegisterAsync(
            request.Email,
            request.Password,
            request.DisplayName,
            request.DateOfBirth,
            request.PhoneNumber,
            acceptedAt,
            acceptedAt,
            cancellationToken);
    }
}
