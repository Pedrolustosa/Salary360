using Salary360.Application.Interfaces.Messaging;
using Salary360.Application.Expenses.Dtos;

namespace Salary360.Application.Expenses.Queries;

public sealed record GetExpenseSeriesQuery(
    int FromYear,
    int FromMonth,
    int ToYear,
    int ToMonth) : IQuery<IReadOnlyList<ExpenseSeriesPointDto>>;
