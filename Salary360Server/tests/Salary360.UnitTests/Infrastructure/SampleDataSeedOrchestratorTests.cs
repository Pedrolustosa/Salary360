using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Salary360.Infrastructure.Seed;
using Salary360.Infrastructure.Seed.Interfaces;

namespace Salary360.UnitTests.Infrastructure;

public sealed class SampleDataSeedOrchestratorTests
{
    [Fact]
    public async Task SeedAsync_DoesNotRunOutsideDevelopment()
    {
        var probe = new ProbeSeeder();
        var provider = BuildProvider(environmentName: "Testing", enabled: true, probe);

        await SampleDataSeedOrchestrator.SeedAsync(provider);

        Assert.Equal(0, probe.CallCount);
    }

    [Fact]
    public async Task SeedAsync_DoesNotRunWhenDisabled()
    {
        var probe = new ProbeSeeder();
        var provider = BuildProvider(environmentName: Environments.Development, enabled: false, probe);

        await SampleDataSeedOrchestrator.SeedAsync(provider);

        Assert.Equal(0, probe.CallCount);
    }

    [Fact]
    public async Task SeedAsync_RunsSeedersInDevelopmentWhenEnabled()
    {
        var probe = new ProbeSeeder();
        var provider = BuildProvider(environmentName: Environments.Development, enabled: true, probe);

        await SampleDataSeedOrchestrator.SeedAsync(provider);

        Assert.Equal(1, probe.CallCount);
    }

    [Fact]
    public async Task SeedAsync_DoesNotRunInProduction()
    {
        var probe = new ProbeSeeder();
        var provider = BuildProvider(environmentName: Environments.Production, enabled: true, probe);

        await SampleDataSeedOrchestrator.SeedAsync(provider);

        Assert.Equal(0, probe.CallCount);
    }

    private static ServiceProvider BuildProvider(string environmentName, bool enabled, ProbeSeeder probe)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IHostEnvironment>(new StubHostEnvironment(environmentName));
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddSingleton(Options.Create(new SeedSettings
        {
            SampleData = new SampleDataSeedSettings
            {
                Enabled = enabled,
                Volume = "medium"
            }
        }));
        services.AddSingleton<ISampleDataSeeder>(probe);
        return services.BuildServiceProvider();
    }

    private sealed class ProbeSeeder : ISampleDataSeeder
    {
        public int Order => 1;

        public int CallCount { get; private set; }

        public Task SeedAsync(CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class StubHostEnvironment(string environmentName) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;

        public string ApplicationName { get; set; } = "Salary360.UnitTests";

        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
