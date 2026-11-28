namespace Salary360.API.Contracts.Responses.Expenses;

public sealed record ExpenseMonthSummaryResponse(
    decimal TotalFixed,
    decimal TotalVariable,
    decimal Total,
    decimal TotalPaid,
    decimal TotalPending,
    int Count,
    IReadOnlyList<ExpenseCategorySummaryResponse> ByCategory,
    bool HasSalary,
    decimal? NetAmount,
    decimal? CommittedPercent,
    decimal? Available);
