using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salary360.Application.Interfaces.Messaging;
using Salary360.Application.Notifications.Commands;

namespace Salary360.API.Controllers;

[ApiController]
[Authorize]
[Route("api/notifications")]
[Tags("Notifications")]
public sealed class NotificationsController(IDispatcher dispatcher) : ControllerBase
{
    [HttpPost("test")]
    [EndpointSummary("Send a test notification")]
    [EndpointDescription("Pushes a test real-time notification to the authenticated user via SignalR.")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> SendTest(CancellationToken cancellationToken)
    {
        await dispatcher.SendAsync(new SendTestNotificationCommand(), cancellationToken);
        return Accepted();
    }
}
