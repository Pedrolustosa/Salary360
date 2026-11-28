using Salary360.Application.Auth;
using Salary360.Application.Interfaces.Identity;
using Salary360.Application.Interfaces.Messaging;
using Salary360.Application.Auth.Dtos;

namespace Salary360.Application.Users.Queries;

public sealed class ListUsersQueryHandler(IIdentityService identityService)
    : IQueryHandler<ListUsersQuery, IReadOnlyList<UserProfileDto>>
{
    public Task<IReadOnlyList<UserProfileDto>> HandleAsync(ListUsersQuery request, CancellationToken cancellationToken = default)
        => identityService.ListUsersAsync(cancellationToken);
}