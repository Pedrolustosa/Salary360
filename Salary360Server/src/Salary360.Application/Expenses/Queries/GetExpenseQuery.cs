using Salary360.Application.Interfaces.Messaging;

namespace Salary360.Application.Expenses.Queries;

public sealed record GetExpenseQuery(Guid Id) : IQuery<ExpenseDto>;
