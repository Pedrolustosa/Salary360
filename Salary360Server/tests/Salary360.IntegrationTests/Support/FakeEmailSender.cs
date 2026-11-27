using System.Collections.Concurrent;
using Salary360.Application.Interfaces.Email;

namespace Salary360.IntegrationTests.Support;

public sealed record SentEmail(string To, string Subject, string HtmlBody);

public sealed class FakeEmailSender : IEmailSender
{
    private readonly ConcurrentQueue<SentEmail> sent = new();

    public IReadOnlyCollection<SentEmail> Sent => sent.ToArray();

    public Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        sent.Enqueue(new SentEmail(to, subject, htmlBody));
        return Task.CompletedTask;
    }
}
