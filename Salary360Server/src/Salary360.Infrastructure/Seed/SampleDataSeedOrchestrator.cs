using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Salary360.Infrastructure.Seed.Interfaces;

namespace Salary360.Infrastructure.Seed;

public static class SampleDataSeedOrchestrator
{
    public static async Task SeedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var services = scope.ServiceProvider;

        var environment = services.GetRequiredService<IHostEnvironment>();
        var settings = services.GetRequiredService<IOptions<SeedSettings>>().Value;
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(SampleDataSeedOrchestrator));

        if (!environment.IsDevelopment() || !settings.SampleData.Enabled)
        {
            logger.LogInformation(
                "Sample data seed skipped (environment={Environment}, enabled={Enabled})",
                environment.EnvironmentName,
                settings.SampleData.Enabled);
            return;
        }

        var seeders = services.GetServices<ISampleDataSeeder>().OrderBy(seeder => seeder.GetOrder()).ToList();
        foreach (var seeder in seeders)
            await seeder.SeedAsync(cancellationToken);
    }
}