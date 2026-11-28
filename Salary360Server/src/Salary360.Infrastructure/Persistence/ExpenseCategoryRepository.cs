using Microsoft.EntityFrameworkCore;
using Salary360.Application.Expenses;
using Salary360.Application.Expenses.Dtos;
using Salary360.Application.Interfaces.Persistence;
using Salary360.Domain.Entities;

namespace Salary360.Infrastructure.Persistence;

public sealed class ExpenseCategoryRepository(AppDbContext context) : IExpenseCategoryRepository
{
    public Task<ExpenseCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => context.ExpenseCategories.FirstOrDefaultAsync(category => category.Id == id, cancellationToken);

    public Task<bool> ExistsWithNameAsync(
        Guid userId, string name, Guid? excludingId = null, CancellationToken cancellationToken = default)
    {
        var normalized = name.Trim().ToLowerInvariant();
        return context.ExpenseCategories.AnyAsync(
            category => category.UserId == userId
                        && category.Name != null
                        && category.Name.ToLower() == normalized
                        && (excludingId == null || category.Id != excludingId),
            cancellationToken);
    }

    public Task<bool> HasSystemKeyAsync(Guid userId, string systemKey, CancellationToken cancellationToken = default)
        => context.ExpenseCategories.AnyAsync(
            category => category.UserId == userId && category.SystemKey == systemKey,
            cancellationToken);

    public Task<int> CountByUserAsync(Guid userId, CancellationToken cancellationToken = default)
        => context.ExpenseCategories.CountAsync(category => category.UserId == userId, cancellationToken);

    public Task<int> CountExpensesAsync(Guid categoryId, CancellationToken cancellationToken = default)
        => context.Expenses.CountAsync(expense => expense.CategoryId == categoryId, cancellationToken);

    public async Task<IReadOnlyList<ExpenseCategoryListItemDto>> ListWithCountsAsync(
        Guid userId, CancellationToken cancellationToken = default)
    {
        var rows = await context.ExpenseCategories
            .AsNoTracking()
            .Where(category => category.UserId == userId)
            .Select(category => new ExpenseCategoryListItemDto(
                category.Id,
                category.Name,
                category.SystemKey,
                category.ColorKey,
                category.IconKey,
                context.Expenses.Count(expense => expense.CategoryId == category.Id),
                category.CreatedAtUtc,
                category.UpdatedAtUtc))
            .OrderBy(category => category.Name ?? category.SystemKey)
            .ToListAsync(cancellationToken);

        return rows;
    }

    public async Task ReassignExpensesAsync(
        Guid fromCategoryId, Guid toCategoryId, CancellationToken cancellationToken = default)
    {
        var expenses = await context.Expenses
            .Where(expense => expense.CategoryId == fromCategoryId)
            .ToListAsync(cancellationToken);

        foreach (var expense in expenses)
            expense.ReassignCategory(toCategoryId);
    }

    public void Add(ExpenseCategory category) => context.ExpenseCategories.Add(category);

    public void Remove(ExpenseCategory category) => context.ExpenseCategories.Remove(category);
}