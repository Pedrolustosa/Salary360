using Salary360.Application.Common;
using Salary360.Application.Interfaces.Messaging;

namespace Salary360.Application.Expenses.Categories.Commands;

public sealed record DeleteExpenseCategoryCommand(Guid Id, Guid? ReassignToCategoryId) : ICommand<Unit>;
