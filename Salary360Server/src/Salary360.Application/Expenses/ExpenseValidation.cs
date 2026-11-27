using Salary360.Domain.Entities;
using Salary360.Domain.Enums;

namespace Salary360.Application.Expenses;

public static class ExpenseValidation
{
    public static bool IsWithinFutureLimit(int year, int month, TimeProvider time)
    {
        if (year is < 1900 or > 2100 || month is < 1 or > 12)
            return true;

        var now = time.GetUtcNow();
        var competence = new DateOnly(year, month, 1);
        var limit = new DateOnly(now.Year, now.Month, 1).AddMonths(Expense.MaxFutureMonths);
        return competence <= limit;
    }

    public static bool IsValidColor(CategoryColor color) => Enum.IsDefined(color);

    public static bool IsValidIcon(CategoryIcon icon) => Enum.IsDefined(icon);

    public static bool IsValidScope(ExpenseChangeScope scope) => Enum.IsDefined(scope);

    public static bool IsValidRecurrence(ExpenseRecurrence recurrence) => Enum.IsDefined(recurrence);

    public static bool IsValidType(ExpenseType type) => Enum.IsDefined(type);

    public static (int Year, int Month) AddMonths(int year, int month, int offset)
    {
        var date = new DateOnly(year, month, 1).AddMonths(offset);
        return (date.Year, date.Month);
    }
}
