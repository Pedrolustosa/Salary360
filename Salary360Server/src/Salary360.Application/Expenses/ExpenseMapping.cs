using Salary360.Domain.Entities;

namespace Salary360.Application.Expenses;

public static class ExpenseMapping
{
    public static ExpenseCategoryDto ToDto(this ExpenseCategory category)
        => new(
            category.Id,
            category.Name,
            category.SystemKey,
            category.ColorKey,
            category.IconKey,
            category.CreatedAtUtc,
            category.UpdatedAtUtc);

    public static ExpenseDto ToDto(this Expense expense)
        => new(
            expense.Id,
            expense.CategoryId,
            expense.Name,
            expense.Type,
            expense.Amount,
            expense.Year,
            expense.Month,
            expense.DueDay,
            expense.IsPaid,
            expense.PaidAtUtc,
            expense.Notes,
            expense.SeriesId,
            expense.CreatedAtUtc,
            expense.UpdatedAtUtc);
}
