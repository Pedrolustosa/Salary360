namespace Salary360.Application.Expenses;

public sealed record PagedExpensesDto(
    IReadOnlyList<ExpenseListItemDto> Items,
    int Total,
    int Page,
    int PageSize);
