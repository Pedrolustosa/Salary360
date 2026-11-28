using FluentValidation;
using Salary360.Application.Auth.Dtos;

namespace Salary360.Application.Auth.Commands;

public sealed class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.DisplayName).ValidDisplayName();
        RuleFor(x => x.DateOfBirth).ValidDateOfBirth();
        RuleFor(x => x.PhoneNumber).ValidOptionalPhone();
        RuleFor(x => x.ProfilePictureUrl).ValidOptionalProfilePictureUrl();
        RuleFor(x => x.Address!).SetValidator(new UserAddressDtoValidator()).When(x => x.Address is not null);
    }
}