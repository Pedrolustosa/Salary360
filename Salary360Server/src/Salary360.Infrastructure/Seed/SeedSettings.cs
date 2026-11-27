namespace Salary360.Infrastructure.Seed;

public sealed class SeedSettings
{
    public const string SectionName = "Seed";

    public SeedUserSettings Admin { get; set; } = new();

    public SeedUserSettings User { get; set; } = new();

    public SampleDataSeedSettings SampleData { get; set; } = new();
}
