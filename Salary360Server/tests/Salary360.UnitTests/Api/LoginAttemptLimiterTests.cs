using Microsoft.Extensions.Options;
using Salary360.API.Services;
using Salary360.API.Settings;
using Salary360.Application.Common.Exceptions;

namespace Salary360.UnitTests.Api;

public sealed class LoginAttemptLimiterTests
{
    private static LoginAttemptLimiter Create(int permitLimit = 3)
        => new(Options.Create(new RateLimitingSettings { LoginPermitLimit = permitLimit, LoginWindowMinutes = 5 }));

    [Fact]
    public void Acquire_WithinLimit_DoesNotThrow()
    {
        using var limiter = Create();

        for (var attempt = 0; attempt < 3; attempt++)
            limiter.Acquire("10.0.0.1", "ana@example.com");
    }

    [Fact]
    public void Acquire_FourthAttemptInWindow_ThrowsWithRetryAfter()
    {
        using var limiter = Create();
        for (var attempt = 0; attempt < 3; attempt++)
            limiter.Acquire("10.0.0.1", "ana@example.com");

        var exception = Assert.Throws<TooManyRequestsException>(() => limiter.Acquire("10.0.0.1", "ana@example.com"));

        Assert.True(exception.RetryAfter > TimeSpan.Zero);
        Assert.True(exception.RetryAfter <= TimeSpan.FromMinutes(5));
    }

    [Fact]
    public void Acquire_EmailIsNormalized()
    {
        using var limiter = Create();
        limiter.Acquire("10.0.0.1", "Ana@Example.com");
        limiter.Acquire("10.0.0.1", " ana@example.com ");
        limiter.Acquire("10.0.0.1", "ANA@EXAMPLE.COM");

        Assert.Throws<TooManyRequestsException>(() => limiter.Acquire("10.0.0.1", "ana@example.com"));
    }

    [Fact]
    public void Acquire_DifferentEmailOrIp_HasItsOwnBudget()
    {
        using var limiter = Create();
        for (var attempt = 0; attempt < 3; attempt++)
            limiter.Acquire("10.0.0.1", "ana@example.com");

        limiter.Acquire("10.0.0.1", "bob@example.com");
        limiter.Acquire("10.0.0.2", "ana@example.com");
    }
}
