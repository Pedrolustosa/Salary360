using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Salary360.Application.Interfaces.Email;

namespace Salary360.IntegrationTests.Support;

public class ApiFactory : WebApplicationFactory<Program>
{
    public const string AdminEmail = "admin@salary360.local";
    public const string AdminPassword = "Admin#Test123";
    public const string UserEmail = "user@salary360.local";
    public const string UserPassword = "User#Test123";

    private readonly string databasePath = Path.Combine(Path.GetTempPath(), $"salary360-tests-{Guid.NewGuid():N}.db");

    public FakeEmailSender Emails { get; } = new();

    protected virtual IReadOnlyDictionary<string, string> Settings => new Dictionary<string, string>
    {
        ["RateLimiting:LoginPermitLimit"] = "100",
        ["RateLimiting:LoginIpPermitLimit"] = "1000",
        ["RateLimiting:RegisterPermitLimit"] = "100",
        ["RateLimiting:ApiPermitLimit"] = "10000"
    };

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        var settings = new Dictionary<string, string>
        {
            ["Jwt:Secret"] = "integration-tests-only-secret-0123456789-abcdef",
            ["Seed:Admin:Password"] = AdminPassword,
            ["Seed:User:Password"] = UserPassword,
            ["Email:Provider"] = "Log",
            ["ConnectionStrings:DefaultConnection"] = $"Data Source={databasePath}"
        };

        foreach (var (key, value) in Settings)
            settings[key] = value;

        foreach (var (key, value) in settings)
            builder.UseSetting(key, value);

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IEmailSender>();
            services.AddSingleton<IEmailSender>(Emails);
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
