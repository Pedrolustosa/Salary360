namespace Salary360.Application.SalaryIntelligence.Dtos;

public sealed record SalaryIntelligenceSummaryDto(
    bool HasEnoughData,
    int RequiredRecords,
    int RecordCount,
    CompetenceDto? From,
    CompetenceDto? To,
    decimal? CurrentGross,
    decimal? CurrentNet,
    decimal? TotalBenefits,
    decimal? TotalDeductions,
    decimal? GrossChangeAbsolute,
    decimal? GrossChangePercent,
    decimal? NetChangeAbsolute,
    decimal? NetChangePercent);
