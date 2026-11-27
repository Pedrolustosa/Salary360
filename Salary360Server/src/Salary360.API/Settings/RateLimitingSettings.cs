using System.ComponentModel.DataAnnotations;

namespace Salary360.API.Settings;

public sealed class RateLimitingSettings
{
    public const string SectionName = "RateLimiting";

    [Range(1, 100)]
    public int LoginPermitLimit { get; init; } = 3;

    [Range(1, 1440)]
    public int LoginWindowMinutes { get; init; } = 5;

    [Range(1, 1000)]
    public int LoginIpPermitLimit { get; init; } = 20;

    [Range(1, 100)]
    public int RegisterPermitLimit { get; init; } = 5;

    [Range(1, 1440)]
    public int RegisterWindowMinutes { get; init; } = 10;

    [Range(1, 10000)]
    public int ApiPermitLimit { get; init; } = 100;
}
