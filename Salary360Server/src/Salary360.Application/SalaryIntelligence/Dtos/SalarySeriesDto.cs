using Salary360.Application.Salaries.Dtos;
namespace Salary360.Application.SalaryIntelligence.Dtos;

public sealed record SalarySeriesPointDto(
    int Year,
    int Month,
    decimal Gross,
    decimal Net,
    decimal Benefits,
    decimal Deductions,
    decimal? GrossChangeAbsolute,
    decimal? GrossChangePercent,
    decimal? NetChangeAbsolute,
    decimal? NetChangePercent);

public sealed record SalarySeriesDto(
    bool HasEnoughData,
    int RequiredRecords,
    int RecordCount,
    CompetenceDto? From,
    CompetenceDto? To,
    IReadOnlyList<SalarySeriesPointDto> Points);
