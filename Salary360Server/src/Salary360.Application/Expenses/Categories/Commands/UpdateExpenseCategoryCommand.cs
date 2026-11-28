using Salary360.Application.Interfaces.Messaging;
using Salary360.Domain.Enums;
using Salary360.Application.Expenses.Dtos;

namespace Salary360.Application.Expenses.Categories.Commands;

public sealed record UpdateExpenseCategoryCommand(
    Guid Id,
    string? Name,
    CategoryColor ColorKey,
    CategoryIcon IconKey) : ICommand<ExpenseCategoryDto>;
