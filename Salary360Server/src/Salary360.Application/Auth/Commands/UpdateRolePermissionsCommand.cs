using Salary360.Application.Interfaces.Messaging;
using Salary360.Application.Auth.Dtos;

namespace Salary360.Application.Auth.Commands;

public sealed record UpdateRolePermissionsCommand(string RoleName, IReadOnlyList<string> Permissions)
    : ICommand<RoleDto>;
