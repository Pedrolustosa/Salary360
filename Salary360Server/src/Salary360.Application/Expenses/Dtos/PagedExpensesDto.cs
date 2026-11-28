namespace Salary360.Application.Expenses.Dtos;

public sealed record PagedExpensesDto(
    IReadOnlyList<ExpenseListItemDto> Items,
    int Total,
    int Page,
    int PageSize);
