using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salary360.Application.Interfaces.Messaging;
using Salary360.Application.Auth;
using Salary360.Application.Auth.Commands;
using Salary360.Application.Common;
using Salary360.Application.Users.Queries;

namespace Salary360.API.Controllers;

[ApiController]
[Route("api/roles")]
[Tags("Roles")]
public sealed class RolesController(IDispatcher dispatcher) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = AppPermissions.Roles.View)]
    [EndpointSummary("List roles")]
    [EndpointDescription("Returns roles and their permission matrix. Requires the roles.view permission.")]
    [ProducesResponseType<IReadOnlyList<RoleDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RoleDto>>> List(CancellationToken cancellationToken)
        => Ok(await dispatcher.SendAsync(new ListRolesQuery(), cancellationToken));

    [HttpPut("{name}/permissions")]
    [Authorize(Policy = AppPermissions.Roles.Update)]
    [EndpointSummary("Update role permissions")]
    [EndpointDescription("Replaces the permission set for a role. The Admin role cannot be modified.")]
    [ProducesResponseType<RoleDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<RoleDto>> UpdatePermissions(
        string name, [FromBody] UpdateRolePermissionsRequest request, CancellationToken cancellationToken)
        => Ok(await dispatcher.SendAsync(new UpdateRolePermissionsCommand(name, request.Permissions), cancellationToken));
}
