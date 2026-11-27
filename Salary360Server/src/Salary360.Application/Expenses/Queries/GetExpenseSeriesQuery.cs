using Salary360.Application.Interfaces.Messaging;

namespace Salary360.Application.Expenses.Queries;

public sealed record GetExpenseSeriesQuery(
    int FromYear,
    int FromMonth,
    int ToYear,
    int ToMonth) : IQuery<IReadOnlyList<ExpenseSeriesPointDto>>;
