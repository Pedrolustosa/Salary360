using FluentValidation;
using Salary360.Application.Interfaces.Identity;
using Salary360.Application.Interfaces.Messaging;

namespace Salary360.Application.Auth.Commands;

public sealed record UpdateProfileCommand(
    string DisplayName,
    DateOnly DateOfBirth,
    string? PhoneNumber,
    string? ProfilePictureUrl,
    UserAddressDto? Address) : ICommand<UserProfileDto>;

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

public sealed class UpdateProfileCommandHandler(IIdentityService identityService, ICurrentUser currentUser)
    : ICommandHandler<UpdateProfileCommand, UserProfileDto>
{
    public Task<UserProfileDto> HandleAsync(UpdateProfileCommand request, CancellationToken cancellationToken = default)
        => identityService.UpdateProfileAsync(
            currentUser.UserId,
            request.DisplayName,
            request.DateOfBirth,
            request.PhoneNumber,
            request.ProfilePictureUrl,
            request.Address,
            cancellationToken);
}
