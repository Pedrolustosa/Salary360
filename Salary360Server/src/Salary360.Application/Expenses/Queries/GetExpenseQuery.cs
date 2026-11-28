using Salary360.Application.Interfaces.Messaging;
using Salary360.Application.Expenses.Dtos;

namespace Salary360.Application.Expenses.Queries;

public sealed record GetExpenseQuery(Guid Id) : IQuery<ExpenseDto>;
