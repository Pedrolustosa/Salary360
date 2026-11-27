using Salary360.Application.Interfaces.Identity;
using Salary360.Application.Interfaces.Messaging;
using Salary360.Application.Auth;

namespace Salary360.Application.Users.Queries;

public sealed record ListRolesQuery : IQuery<IReadOnlyList<RoleDto>>;

public sealed class ListRolesQueryHandler(IIdentityService identityService)
    : IQueryHandler<ListRolesQuery, IReadOnlyList<RoleDto>>
{
    public Task<IReadOnlyList<RoleDto>> HandleAsync(ListRolesQuery request, CancellationToken cancellationToken = default)
        => identityService.ListRolesAsync(cancellationToken);
}
