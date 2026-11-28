namespace Salary360.Application.Expenses.Dtos;

public sealed record ExpenseSeriesPointDto(
    int Year,
    int Month,
    decimal TotalFixed,
    decimal TotalVariable);
