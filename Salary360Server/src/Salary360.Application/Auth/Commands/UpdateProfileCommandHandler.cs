using Salary360.Application.Interfaces.Identity;
using Salary360.Application.Interfaces.Messaging;
using Salary360.Application.Auth.Dtos;

namespace Salary360.Application.Auth.Commands;

public sealed class UpdateProfileCommandHandler(IIdentityService identityService, ICurrentUser currentUser)
    : ICommandHandler<UpdateProfileCommand, UserProfileDto>
{
    public Task<UserProfileDto> HandleAsync(UpdateProfileCommand request, CancellationToken cancellationToken = default)
        => identityService.UpdateProfileAsync(
            currentUser.GetUserId(),
            request.DisplayName,
            request.DateOfBirth,
            request.PhoneNumber,
            request.ProfilePictureUrl,
            request.Address,
            cancellationToken);
}