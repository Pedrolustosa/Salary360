using Salary360.Domain.Enums;

namespace Salary360.API.Contracts.Requests.Expenses;

public sealed record CreateExpenseRequest(
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
    int? RecurrenceCount);
