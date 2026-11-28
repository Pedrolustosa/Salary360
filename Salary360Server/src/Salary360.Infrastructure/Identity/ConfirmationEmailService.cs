using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Buffers.Text;
using System.Text;
using Salary360.Application.Common;
using Salary360.Application.Interfaces.Email;
using Salary360.Infrastructure.Email;
using Salary360.Infrastructure.Enums;

namespace Salary360.Infrastructure.Identity;

public sealed class ConfirmationEmailService(
    UserManager<ApplicationUser> userManager,
    IEmailSender emailSender,
    IEmailTemplateRenderer templateRenderer,
    IMemoryCache cache,
    IOptions<ProjectSettings> projectOptions,
    IOptions<ClientSettings> clientOptions,
    IOptions<EmailSettings> emailOptions,
    ILogger<ConfirmationEmailService> logger)
{
    public async Task SendAsync(ApplicationUser user, ConfirmationEmailKind kind, CancellationToken cancellationToken = default)
    {
        if (kind == ConfirmationEmailKind.Resend && IsThrottled(user))
        {
            logger.LogInformation("Confirmation e-mail for user {UserId} skipped: resend interval has not elapsed", user.Id);
            return;
        }

        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = Base64Url.EncodeToString(Encoding.UTF8.GetBytes(token));
        var link = $"{clientOptions.Value.BaseUrl.TrimEnd('/')}/confirm-email?userId={user.Id}&token={encodedToken}";

        var projectName = projectOptions.Value.Name;
        var values = new Dictionary<string, string>
        {
            ["ProjectName"] = projectName,
            ["UserName"] = user.DisplayName,
            ["ConfirmationLink"] = link
        };

        var (template, subject) = kind == ConfirmationEmailKind.AccountCreated
            ? (EmailTemplates.AccountConfirmation, $"Confirm your {projectName} registration")
            : (EmailTemplates.ResendConfirmation, $"New confirmation link - {projectName}");

        await emailSender.SendAsync(user.Email!, subject, templateRenderer.Render(template, values), cancellationToken);
        logger.LogInformation("Confirmation e-mail ({Kind}) sent for user {UserId}", kind, user.Id);
    }

    private bool IsThrottled(ApplicationUser user)
    {
        var key = $"resend-confirmation:{user.Id}";
        if (cache.TryGetValue(key, out _))
            return true;

        var interval = TimeSpan.FromSeconds(emailOptions.Value.ResendIntervalSeconds);
        if (interval > TimeSpan.Zero)
            cache.Set(key, true, interval);

        return false;
    }
}