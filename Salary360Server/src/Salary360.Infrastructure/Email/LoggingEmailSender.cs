using Microsoft.Extensions.Logging;
using Salary360.Application.Interfaces.Email;

namespace Salary360.Infrastructure.Email;

public sealed class LoggingEmailSender(ILogger<LoggingEmailSender> logger) : IEmailSender
{
    public Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("E-mail (not sent, Email:Provider=Log) to {To} with subject {Subject}: {Body}", to, subject, htmlBody);
        return Task.CompletedTask;
    }
}
