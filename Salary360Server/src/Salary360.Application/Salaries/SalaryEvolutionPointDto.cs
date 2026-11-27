namespace Salary360.Application.Salaries;

public sealed record SalaryEvolutionPointDto(
    int Year,
    decimal? Gross,
    decimal? Benefits,
    decimal? Deductions,
    decimal? Net,
    decimal? AverageNet = null,
    int? MonthsCount = null);
