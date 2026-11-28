using System.Threading.RateLimiting;
using Microsoft.Extensions.Options;
using Salary360.API.Interfaces;
using Salary360.API.Settings;
using Salary360.Application.Common.Exceptions;

namespace Salary360.API.Services;

public sealed class LoginAttemptLimiter : ILoginAttemptLimiter, IDisposable
{
    private readonly PartitionedRateLimiter<string> limiter;
    private readonly TimeSpan window;

    public LoginAttemptLimiter(IOptions<RateLimitingSettings> options)
    {
        var settings = options.Value;
        window = TimeSpan.FromMinutes(settings.LoginWindowMinutes);

        limiter = PartitionedRateLimiter.Create<string, string>(key => RateLimitPartition.GetFixedWindowLimiter(
            key,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = settings.LoginPermitLimit,
                Window = window,
                QueueLimit = 0,
                AutoReplenishment = true
            }));
    }

    public void Acquire(string ipAddress, string email)
    {
        var key = $"{ipAddress}|{email.Trim().ToLowerInvariant()}";
        using var lease = limiter.AttemptAcquire(key);

        if (lease.IsAcquired)
            return;

        var retryAfter = lease.TryGetMetadata(MetadataName.RetryAfter, out var value) ? value : window;
        throw new TooManyRequestsException(retryAfter);
    }

    public void Dispose() => limiter.Dispose();
}