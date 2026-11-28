namespace Salary360.Application.Expenses.Dtos;

public sealed record ExpenseMonthSummaryDto(
    decimal TotalFixed,
    decimal TotalVariable,
    decimal Total,
    decimal TotalPaid,
    decimal TotalPending,
    int Count,
    IReadOnlyList<ExpenseCategorySummaryDto> ByCategory,
    bool HasSalary,
    decimal? NetAmount,
    decimal? CommittedPercent,
    decimal? Available);
