using FluentValidation;

namespace Salary360.Application.Auth.Commands;

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