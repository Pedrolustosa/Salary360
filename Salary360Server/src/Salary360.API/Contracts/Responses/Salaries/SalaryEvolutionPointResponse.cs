namespace Salary360.API.Contracts.Responses.Salaries;

public sealed record SalaryEvolutionPointResponse(
    int Year,
    decimal? Gross,
    decimal? Benefits,
    decimal? Deductions,
    decimal? Net,
    decimal? AverageNet = null,
    int? MonthsCount = null);
