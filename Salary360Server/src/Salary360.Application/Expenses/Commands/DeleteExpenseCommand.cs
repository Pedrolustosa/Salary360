using Salary360.Application.Common;
using Salary360.Application.Interfaces.Messaging;
using Salary360.Domain.Enums;

namespace Salary360.Application.Expenses.Commands;

public sealed record DeleteExpenseCommand(Guid Id, ExpenseChangeScope Scope) : ICommand<Unit>;
