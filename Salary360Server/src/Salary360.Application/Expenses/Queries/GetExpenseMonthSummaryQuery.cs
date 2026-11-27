using Salary360.Application.Interfaces.Messaging;

namespace Salary360.Application.Expenses.Queries;

public sealed record GetExpenseMonthSummaryQuery(int Year, int Month) : IQuery<ExpenseMonthSummaryDto>;
