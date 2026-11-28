namespace Salary360.Application.Notifications;

public sealed record NotificationMessage(
    Guid Id,
    string Type,
    string Title,
    string Message,
    DateTime CreatedAtUtc)
{
    public static NotificationMessage Create(string type, string title, string message)
        => new(Guid.NewGuid(), type, title, message, DateTime.UtcNow);
}