using FluentValidation;
using Salary360.Application.Interfaces.Identity;
using Salary360.Application.Interfaces.Messaging;
using Salary360.Domain.Common;
using Salary360.Domain.Enums;

namespace Salary360.Application.Auth.Commands;

public sealed record SetUserStatusCommand(Guid UserId, UserStatus Status) : ICommand<UserProfileDto>;

public sealed class SetUserStatusCommandValidator : AbstractValidator<SetUserStatusCommand>
{
    public SetUserStatusCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Status).IsInEnum();
    }
}

public sealed class SetUserStatusCommandHandler(IIdentityService identityService, ICurrentUser currentUser)
    : ICommandHandler<SetUserStatusCommand, UserProfileDto>
{
    public Task<UserProfileDto> HandleAsync(SetUserStatusCommand request, CancellationToken cancellationToken = default)
    {
        if (request.UserId == currentUser.UserId && request.Status == UserStatus.Inactive)
            throw new DomainException("You cannot deactivate your own account.", "cannot_deactivate_self");

        return identityService.SetStatusAsync(request.UserId, request.Status, cancellationToken);
    }
}
