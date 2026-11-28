using Salary360.Application.Interfaces.Messaging;
using Salary360.Application.Expenses.Dtos;

namespace Salary360.Application.Expenses.Commands;

public sealed record SetExpensePaidCommand(Guid Id, bool IsPaid) : ICommand<ExpenseDto>;
