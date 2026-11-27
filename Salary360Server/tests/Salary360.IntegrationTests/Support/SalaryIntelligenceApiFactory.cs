using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Salary360.Application.Common.Interfaces;
using Salary360.Application.Interfaces.Email;

namespace Salary360.IntegrationTests.Support;

public sealed class SalaryIntelligenceApiFactory : WebApplicationFactory<Program>
{
    public const string AdminEmail = ApiFactory.AdminEmail;
    public const string AdminPassword = ApiFactory.AdminPassword;
    public const string UserEmail = ApiFactory.UserEmail;
    public const string UserPassword = ApiFactory.UserPassword;

    private readonly string databasePath = Path.Combine(Path.GetTempPath(), $"salary360-intel-{Guid.NewGuid():N}.db");

    public FakeEmailSender Emails { get; } = new();

    public FakeInflationProvider Inflation { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        var settings = new Dictionary<string, string>
        {
            ["Jwt:Secret"] = "integration-tests-only-secret-0123456789-abcdef",
            ["Seed:Admin:Password"] = AdminPassword,
            ["Seed:User:Password"] = UserPassword,
            ["Email:Provider"] = "Log",
            ["ConnectionStrings:DefaultConnection"] = $"Data Source={databasePath}",
            ["RateLimiting:LoginPermitLimit"] = "100",
            ["RateLimiting:LoginIpPermitLimit"] = "1000",
            ["RateLimiting:RegisterPermitLimit"] = "100",
            ["RateLimiting:ApiPermitLimit"] = "10000",
            ["Ibge:BaseUrl"] = "https://apisidra.ibge.gov.br",
            ["Ibge:Table"] = "1737",
            ["Ibge:Variable"] = "2266",
            ["Ibge:CacheHours"] = "12",
            ["Ibge:StaleCacheHours"] = "168",
            ["Ibge:TimeoutSeconds"] = "10"
        };

        foreach (var (key, value) in settings)
            builder.UseSetting(key, value);

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IEmailSender>();
            services.AddSingleton<IEmailSender>(Emails);
            services.RemoveAll<IInflationProvider>();
            services.AddSingleton<IInflationProvider>(Inflation);
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (!disposing)
            return;

        foreach (var suffix in new[] { string.Empty, "-shm", "-wal" })
        {
            var path = databasePath + suffix;
            try
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch (IOException)
            {
            }
        }
    }
}
