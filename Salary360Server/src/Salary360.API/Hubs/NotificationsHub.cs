using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Salary360.API.Hubs;

[Authorize]
public sealed class NotificationsHub : Hub
{
    public const string Route = "/hubs/notifications";
    public const string ReceiveMethod = "ReceiveNotification";
}