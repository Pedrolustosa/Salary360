using FluentValidation;
using Salary360.Application.Common;
using Salary360.Application.Interfaces.Identity;
using Salary360.Application.Interfaces.Messaging;

namespace Salary360.Application.Auth.Commands;

public sealed record ResendConfirmationCommand(string Email) : ICommand<Unit>;

public sealed class ResendConfirmationCommandValidator : AbstractValidator<ResendConfirmationCommand>
{
    public ResendConfirmationCommandValidator()
    {
        RuleFor(x => x.Email).ValidEmail();
    }
}

public sealed class ResendConfirmationCommandHandler(IIdentityService identityService)
    : ICommandHandler<ResendConfirmationCommand, Unit>
{
    public async Task<Unit> HandleAsync(ResendConfirmationCommand request, CancellationToken cancellationToken = default)
    {
        await identityService.ResendConfirmationAsync(request.Email, cancellationToken);
        return Unit.Value;
    }
}
