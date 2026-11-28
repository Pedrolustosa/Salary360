using FluentValidation;

namespace Salary360.Application.Auth.Commands;

public sealed class ResendConfirmationCommandValidator : AbstractValidator<ResendConfirmationCommand>
{
    public ResendConfirmationCommandValidator()
    {
        RuleFor(x => x.Email).ValidEmail();
    }
}