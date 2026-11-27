namespace Salary360.Application.SalaryIntelligence.Dtos;

public sealed record InflationCoverageDto(
    int FromYear,
    int FromMonth,
    int ToYear,
    int ToMonth,
    bool IsPartial);

public sealed record PurchasingPowerPointDto(
    int Year,
    int Month,
    decimal Actual,
    decimal? InflationAdjusted);

public sealed record PurchasingPowerDto(
    bool HasEnoughData,
    int RequiredRecords,
    int RecordCount,
    CompetenceDto? From,
    CompetenceDto? To,
    bool InflationAvailable,
    bool IsStale,
    InflationCoverageDto? InflationCoverage,
    decimal? NominalChangePercent,
    decimal? IpcaAccumulatedPercent,
    decimal? RealGainPercent,
    decimal? RealGainAbsolute,
    IReadOnlyList<PurchasingPowerPointDto> Series);
