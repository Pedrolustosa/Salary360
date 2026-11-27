using Salary360.Application.Interfaces.Messaging;
using Salary360.Domain.Enums;

namespace Salary360.Application.Expenses.Commands;

public sealed record CreateExpenseCommand(
    Guid CategoryId,
    string Name,
    ExpenseType Type,
    decimal Amount,
    int Year,
    int Month,
    int? DueDay,
    bool IsPaid,
    string? Notes,
    ExpenseRecurrence Recurrence,
    int? RecurrenceCount) : ICommand<IReadOnlyList<ExpenseDto>>;
