using Salary360.Application.Interfaces.Identity;
using Salary360.Application.Interfaces.Messaging;
using Salary360.Application.Auth.Dtos;

namespace Salary360.Application.Auth.Commands;

public sealed class UpdateRolePermissionsCommandHandler(IIdentityService identityService)
    : ICommandHandler<UpdateRolePermissionsCommand, RoleDto>
{
    public Task<RoleDto> HandleAsync(UpdateRolePermissionsCommand request, CancellationToken cancellationToken = default)
        => identityService.UpdateRolePermissionsAsync(request.RoleName, request.Permissions, cancellationToken);
}