using FluentValidation;
namespace Salary360.Application.Auth.Dtos;

public sealed class UserAddressDtoValidator : AbstractValidator<UserAddressDto>
{
    public UserAddressDtoValidator()
    {
        RuleFor(x => x.PostalCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Street).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Number).NotEmpty().MaximumLength(10);
        RuleFor(x => x.Complement).MaximumLength(100);
        RuleFor(x => x.Neighborhood).NotEmpty().MaximumLength(100);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.State).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Country).NotEmpty().Matches("^[A-Za-z]{2}$");
        RuleFor(x => x.PostalCode).Matches(@"^[A-Za-z0-9 \-]{3,20}$").When(x => !string.IsNullOrEmpty(x.PostalCode));
    }
}