using Salary360.Application.Common;
using Salary360.Application.Interfaces.Identity;
using Salary360.Application.Interfaces.Messaging;
using Salary360.Application.Interfaces.Notifications;

namespace Salary360.Application.Notifications.Commands;

public sealed record SendTestNotificationCommand : ICommand<Unit>;

public sealed class SendTestNotificationCommandHandler(INotificationPublisher publisher, ICurrentUser currentUser)
    : ICommandHandler<SendTestNotificationCommand, Unit>
{
    public async Task<Unit> HandleAsync(SendTestNotificationCommand request, CancellationToken cancellationToken = default)
    {
        var notification = NotificationMessage.Create(
            "test",
            "Test notification",
            "Real-time notifications are working.");

        await publisher.SendToUserAsync(currentUser.UserId, notification, cancellationToken);
        return Unit.Value;
    }
}
