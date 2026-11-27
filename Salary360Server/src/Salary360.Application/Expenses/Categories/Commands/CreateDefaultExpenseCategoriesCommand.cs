using Salary360.Application.Interfaces.Messaging;

namespace Salary360.Application.Expenses.Categories.Commands;

public sealed record CreateDefaultExpenseCategoriesCommand : ICommand<CreateDefaultsResultDto>;
