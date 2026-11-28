namespace Salary360.API.Contracts.Responses.SalaryIntelligence;

public sealed record InflationCoverageResponse(
    int FromYear,
    int FromMonth,
    int ToYear,
    int ToMonth,
    bool IsPartial);

public sealed record PurchasingPowerPointResponse(
    int Year,
    int Month,
    decimal Actual,
    decimal? InflationAdjusted);

public sealed record PurchasingPowerResponse(
    bool HasEnoughData,
    int RequiredRecords,
    int RecordCount,
    CompetenceResponse? From,
    CompetenceResponse? To,
    bool InflationAvailable,
    bool IsStale,
    InflationCoverageResponse? InflationCoverage,
    decimal? NominalChangePercent,
    decimal? IpcaAccumulatedPercent,
    decimal? RealGainPercent,
    decimal? RealGainAbsolute,
    IReadOnlyList<PurchasingPowerPointResponse> Series);
