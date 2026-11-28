namespace Salary360.API.Contracts.Responses.Expenses;

public sealed record ExpenseSeriesPointResponse(
    int Year,
    int Month,
    decimal TotalFixed,
    decimal TotalVariable);
