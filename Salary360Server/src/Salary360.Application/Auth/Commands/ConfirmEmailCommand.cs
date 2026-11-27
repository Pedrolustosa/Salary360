using FluentValidation;
using Salary360.Application.Common;
using Salary360.Application.Interfaces.Identity;
using Salary360.Application.Interfaces.Messaging;

namespace Salary360.Application.Auth.Commands;

public sealed record ConfirmEmailCommand(Guid UserId, string Token) : ICommand<Unit>;

public sealed class ConfirmEmailCommandValidator : AbstractValidator<ConfirmEmailCommand>
{
    public ConfirmEmailCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Token).NotEmpty().MaximumLength(2048);
    }
}

public sealed class ConfirmEmailCommandHandler(IIdentityService identityService)
    : ICommandHandler<ConfirmEmailCommand, Unit>
{
    public async Task<Unit> HandleAsync(ConfirmEmailCommand request, CancellationToken cancellationToken = default)
    {
        await identityService.ConfirmEmailAsync(request.UserId, request.Token, cancellationToken);
        return Unit.Value;
    }
}
