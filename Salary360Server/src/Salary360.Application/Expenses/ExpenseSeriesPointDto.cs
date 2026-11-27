namespace Salary360.Application.Expenses;

public sealed record ExpenseSeriesPointDto(
    int Year,
    int Month,
    decimal TotalFixed,
    decimal TotalVariable);
