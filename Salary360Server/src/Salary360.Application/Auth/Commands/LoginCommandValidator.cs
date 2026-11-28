using FluentValidation;

namespace Salary360.Application.Auth.Commands;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).ValidEmail();
        RuleFor(x => x.Password).NotEmpty().MaximumLength(128);
    }
}