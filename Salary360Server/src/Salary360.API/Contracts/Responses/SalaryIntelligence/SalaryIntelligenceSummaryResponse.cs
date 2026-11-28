namespace Salary360.API.Contracts.Responses.SalaryIntelligence;

public sealed record SalaryIntelligenceSummaryResponse(
    bool HasEnoughData,
    int RequiredRecords,
    int RecordCount,
    CompetenceResponse? From,
    CompetenceResponse? To,
    decimal? CurrentGross,
    decimal? CurrentNet,
    decimal? TotalBenefits,
    decimal? TotalDeductions,
    decimal? GrossChangeAbsolute,
    decimal? GrossChangePercent,
    decimal? NetChangeAbsolute,
    decimal? NetChangePercent);
