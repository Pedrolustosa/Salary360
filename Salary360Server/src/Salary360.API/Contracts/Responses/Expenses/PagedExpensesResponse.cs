namespace Salary360.API.Contracts.Responses.Expenses;

public sealed record PagedExpensesResponse(
    IReadOnlyList<ExpenseListItemResponse> Items,
    int Total,
    int Page,
    int PageSize);
