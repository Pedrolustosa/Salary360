using Microsoft.AspNetCore.SignalR;
using Salary360.API.Hubs;
using Salary360.Application.Interfaces.Notifications;
using Salary360.Application.Notifications;

namespace Salary360.API.Services;

public sealed class SignalRNotificationPublisher(IHubContext<NotificationsHub> hubContext) : INotificationPublisher
{
    public Task SendToUserAsync(Guid userId, NotificationMessage notification, CancellationToken cancellationToken = default)
        => hubContext.Clients.User(userId.ToString()).SendAsync(NotificationsHub.ReceiveMethod, notification, cancellationToken);

    public Task BroadcastAsync(NotificationMessage notification, CancellationToken cancellationToken = default)
        => hubContext.Clients.All.SendAsync(NotificationsHub.ReceiveMethod, notification, cancellationToken);
}