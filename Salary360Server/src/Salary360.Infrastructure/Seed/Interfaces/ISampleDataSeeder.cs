namespace Salary360.Infrastructure.Seed.Interfaces;

public interface ISampleDataSeeder
{
    int GetOrder();

    Task SeedAsync(CancellationToken cancellationToken = default);
}