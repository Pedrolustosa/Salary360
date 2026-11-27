using Salary360.Application.Interfaces.Identity;
using Salary360.Application.Interfaces.Messaging;

namespace Salary360.Application.Auth.Queries;

public sealed record GetCurrentUserQuery : IQuery<UserProfileDto>;

public sealed class GetCurrentUserQueryHandler(IIdentityService identityService, ICurrentUser currentUser)
    : IQueryHandler<GetCurrentUserQuery, UserProfileDto>
{
    public Task<UserProfileDto> HandleAsync(GetCurrentUserQuery request, CancellationToken cancellationToken = default)
        => identityService.GetProfileAsync(currentUser.UserId, cancellationToken);
}
