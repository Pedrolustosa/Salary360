using Salary360.Infrastructure.Enums;

namespace Salary360.Infrastructure.Seed;

public static class SampleDataVolumeCounts
{
    public static SampleDataVolume Parse(string? volume)
    {
        if (string.Equals(volume, "small", StringComparison.OrdinalIgnoreCase)
            || string.Equals(volume, "pequeno", StringComparison.OrdinalIgnoreCase))
            return SampleDataVolume.Small;

        if (string.Equals(volume, "large", StringComparison.OrdinalIgnoreCase)
            || string.Equals(volume, "grande", StringComparison.OrdinalIgnoreCase))
            return SampleDataVolume.Large;

        return SampleDataVolume.Medium;
    }

    public static int UserRecordCount(SampleDataVolume volume)
        => volume switch
        {
            SampleDataVolume.Small => 12,
            SampleDataVolume.Large => 120,
            _ => 48
        };

    public static int AdminRecordCount(SampleDataVolume volume)
        => volume switch
        {
            SampleDataVolume.Small => 6,
            SampleDataVolume.Large => 36,
            _ => 18
        };
}
