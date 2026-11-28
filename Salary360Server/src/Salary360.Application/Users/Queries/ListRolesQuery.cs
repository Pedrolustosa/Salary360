using Salary360.Application.Auth;
using Salary360.Application.Interfaces.Messaging;
using Salary360.Application.Auth.Dtos;

namespace Salary360.Application.Users.Queries;

public sealed record ListRolesQuery : IQuery<IReadOnlyList<RoleDto>>;
