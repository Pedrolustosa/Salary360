namespace Salary360.API.Contracts.Responses.SalaryIntelligence;

public sealed record SalarySeriesPointResponse(
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

public sealed record SalarySeriesResponse(
    bool HasEnoughData,
    int RequiredRecords,
    int RecordCount,
    CompetenceResponse? From,
    CompetenceResponse? To,
    IReadOnlyList<SalarySeriesPointResponse> Points);
