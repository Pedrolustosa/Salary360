namespace Salary360.Infrastructure.Seed.Interfaces;

public interface ISampleDataSeeder
{
    int Order { get; }

    Task SeedAsync(CancellationToken cancellationToken = default);
}
