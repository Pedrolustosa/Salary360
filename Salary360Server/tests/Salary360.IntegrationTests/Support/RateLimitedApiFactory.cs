namespace Salary360.IntegrationTests.Support;

public sealed class RateLimitedApiFactory : ApiFactory
{
    protected override IReadOnlyDictionary<string, string> Settings => new Dictionary<string, string>
    {
        ["RateLimiting:LoginPermitLimit"] = "3",
        ["RateLimiting:LoginWindowMinutes"] = "5",
        ["RateLimiting:LoginIpPermitLimit"] = "1000",
        ["RateLimiting:RegisterPermitLimit"] = "100",
        ["RateLimiting:ApiPermitLimit"] = "10000"
    };
}
