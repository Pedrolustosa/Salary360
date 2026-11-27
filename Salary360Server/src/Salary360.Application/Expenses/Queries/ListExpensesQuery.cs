using Salary360.Application.Interfaces.Messaging;
using Salary360.Domain.Enums;

namespace Salary360.Application.Expenses.Queries;

public sealed record ListExpensesQuery(
    int Year,
    int Month,
    Guid? CategoryId,
    ExpenseType? Type,
    bool? IsPaid,
    int Page,
    int PageSize) : IQuery<PagedExpensesDto>;
