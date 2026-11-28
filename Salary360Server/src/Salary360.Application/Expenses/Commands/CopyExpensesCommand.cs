using Salary360.Application.Interfaces.Messaging;
using Salary360.Application.Expenses.Dtos;

namespace Salary360.Application.Expenses.Commands;

public sealed record CopyExpensesCommand(
    int FromYear,
    int FromMonth,
    int ToYear,
    int ToMonth,
    bool OnlyFixed) : ICommand<CopyExpensesResultDto>;
