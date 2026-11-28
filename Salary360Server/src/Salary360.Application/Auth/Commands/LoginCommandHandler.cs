using Salary360.Application.Interfaces.Identity;
using Salary360.Application.Interfaces.Messaging;
using Salary360.Application.Auth.Dtos;

namespace Salary360.Application.Auth.Commands;

public sealed class LoginCommandHandler(IIdentityService identityService)
    : ICommandHandler<LoginCommand, AuthResponse>
{
    public Task<AuthResponse> HandleAsync(LoginCommand request, CancellationToken cancellationToken = default)
        => identityService.LoginAsync(request.Email, request.Password, cancellationToken);
}