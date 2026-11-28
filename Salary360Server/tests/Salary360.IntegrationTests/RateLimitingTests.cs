using System.Net;
using System.Net.Http.Json;
using Salary360.IntegrationTests.Support;

namespace Salary360.IntegrationTests;

public sealed class RateLimitingTests(RateLimitedApiFactory factory) : IClassFixture<RateLimitedApiFactory>
{
    [Fact]
    public async Task Login_FourthAttemptWithinTheWindow_Returns429WithRetryAfter()
    {
        var client = factory.CreateClient();
        var credentials = new { email = "ghost@example.com", password = "Wrong#Pass1" };

        for (var attempt = 1; attempt <= 3; attempt++)
        {
            var response = await client.PostAsJsonAsync("/api/auth/login", credentials);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        var blocked = await client.PostAsJsonAsync("/api/auth/login", credentials);

        Assert.Equal(HttpStatusCode.TooManyRequests, blocked.StatusCode);
        Assert.True(blocked.Headers.TryGetValues("Retry-After", out var retryAfter));
        Assert.True(int.Parse(retryAfter!.First()) > 0);
        var body = await blocked.ReadJsonAsync();
        Assert.Equal("too_many_requests", body.GetProperty("code").GetString());
        Assert.True(body.GetProperty("retryAfterSeconds").GetInt32() > 0);
    }

    [Fact]
    public async Task Login_AnotherEmail_IsNotAffectedByTheBlockedOne()
    {
        var client = factory.CreateClient();
        var blockedEmail = new { email = "blocked@example.com", password = "Wrong#Pass1" };
        for (var attempt = 1; attempt <= 4; attempt++)
            await client.PostAsJsonAsync("/api/auth/login", blockedEmail);

        var other = await client.PostAsJsonAsync("/api/auth/login", new { email = ApiFactory.AdminEmail, password = ApiFactory.AdminPassword });

        Assert.Equal(HttpStatusCode.OK, other.StatusCode);
    }
}