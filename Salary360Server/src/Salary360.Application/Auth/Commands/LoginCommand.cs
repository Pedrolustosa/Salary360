using FluentValidation;
using Salary360.Application.Interfaces.Identity;
using Salary360.Application.Interfaces.Messaging;

namespace Salary360.Application.Auth.Commands;

public sealed record LoginCommand(string Email, string Password) : ICommand<AuthResponse>;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).ValidEmail();
        RuleFor(x => x.Password).NotEmpty().MaximumLength(128);
    }
}

public sealed class LoginCommandHandler(IIdentityService identityService)
    : ICommandHandler<LoginCommand, AuthResponse>
{
    public Task<AuthResponse> HandleAsync(LoginCommand request, CancellationToken cancellationToken = default)
        => identityService.LoginAsync(request.Email, request.Password, cancellationToken);
}
