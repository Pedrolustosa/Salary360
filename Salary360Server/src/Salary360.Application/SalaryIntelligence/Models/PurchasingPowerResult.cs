namespace Salary360.Application.SalaryIntelligence.Models;

public sealed record InflationCoverage(
    int FromYear,
    int FromMonth,
    int ToYear,
    int ToMonth,
    bool IsPartial);

public sealed record PurchasingPowerPoint(
    int Year,
    int Month,
    decimal Actual,
    decimal? InflationAdjusted);

public sealed record PurchasingPowerResult(
    bool InflationAvailable,
    bool IsStale,
    InflationCoverage? Coverage,
    decimal NominalChangePercent,
    decimal? IpcaAccumulatedPercent,
    decimal? RealGainPercent,
    decimal? RealGainAbsolute,
    IReadOnlyList<PurchasingPowerPoint> Series);
