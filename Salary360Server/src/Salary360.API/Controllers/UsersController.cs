using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salary360.Application.Interfaces.Messaging;
using Salary360.Application.Auth;
using Salary360.Application.Auth.Commands;
using Salary360.Application.Common;
using Salary360.Application.Users.Queries;

namespace Salary360.API.Controllers;

[ApiController]
[Route("api/users")]
[Tags("Users")]
public sealed class UsersController(IDispatcher dispatcher) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = AppPermissions.Users.View)]
    [EndpointSummary("List users")]
    [EndpointDescription("Returns all users. Requires the users.view permission.")]
    [ProducesResponseType<IReadOnlyList<UserProfileDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<UserProfileDto>>> List(CancellationToken cancellationToken)
        => Ok(await dispatcher.SendAsync(new ListUsersQuery(), cancellationToken));

    [HttpPut("{id:guid}/status")]
    [Authorize(Policy = AppPermissions.Users.Update)]
    [EndpointSummary("Set user status")]
    [EndpointDescription("Activates or deactivates a user. Administrators cannot deactivate their own account.")]
    [ProducesResponseType<UserProfileDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<UserProfileDto>> SetStatus(
        Guid id, [FromBody] SetUserStatusRequest request, CancellationToken cancellationToken)
        => Ok(await dispatcher.SendAsync(new SetUserStatusCommand(id, request.Status), cancellationToken));
}
