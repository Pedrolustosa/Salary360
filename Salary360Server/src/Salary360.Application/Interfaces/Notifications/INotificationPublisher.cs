using Salary360.Application.Notifications;

namespace Salary360.Application.Interfaces.Notifications;

public interface INotificationPublisher
{
    Task SendToUserAsync(Guid userId, NotificationMessage notification, CancellationToken cancellationToken = default);

    Task BroadcastAsync(NotificationMessage notification, CancellationToken cancellationToken = default);
}