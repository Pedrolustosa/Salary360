using Salary360.Application.Interfaces.Messaging;

namespace Salary360.Application.Expenses.Commands;

public sealed record SetExpensePaidCommand(Guid Id, bool IsPaid) : ICommand<ExpenseDto>;
