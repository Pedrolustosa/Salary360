using FluentValidation;

namespace Salary360.Application.Auth.Commands;

public sealed class SetUserStatusCommandValidator : AbstractValidator<SetUserStatusCommand>
{
    public SetUserStatusCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Status).IsInEnum();
    }
}