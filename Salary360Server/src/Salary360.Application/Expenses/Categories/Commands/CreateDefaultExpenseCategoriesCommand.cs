using Salary360.Application.Interfaces.Messaging;
using Salary360.Application.Expenses.Dtos;

namespace Salary360.Application.Expenses.Categories.Commands;

public sealed record CreateDefaultExpenseCategoriesCommand : ICommand<CreateDefaultsResultDto>;
