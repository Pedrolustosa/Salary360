using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salary360.API.Contracts.Requests.Roles;
using Salary360.Application.Auth;
using Salary360.Application.Auth.Commands;
using Salary360.Application.Common;
using Salary360.Application.Interfaces.Messaging;
using Salary360.Application.Users.Queries;
using Salary360.API.Contracts.Responses.Auth;

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
    [ProducesResponseType<IReadOnlyList<RoleResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RoleResponse>>> List(CancellationToken cancellationToken)
        => Ok((await dispatcher.SendAsync(new ListRolesQuery(), cancellationToken)).ToResponse());

    [HttpPut("{name}/permissions")]
    [Authorize(Policy = AppPermissions.Roles.Update)]
    [EndpointSummary("Update role permissions")]
    [EndpointDescription("Replaces the permission set for a role. The Admin role cannot be modified.")]
    [ProducesResponseType<RoleResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<RoleResponse>> UpdatePermissions(
        string name, [FromBody] UpdateRolePermissionsRequest request, CancellationToken cancellationToken)
        => Ok((await dispatcher.SendAsync(new UpdateRolePermissionsCommand(name, request.Permissions), cancellationToken)).ToResponse());
}