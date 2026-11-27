using Salary360.Application.Expenses;
using Salary360.Domain.Entities;

namespace Salary360.Application.Interfaces.Persistence;

public interface IExpenseCategoryRepository
{
    Task<ExpenseCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsWithNameAsync(
        Guid userId, string name, Guid? excludingId = null, CancellationToken cancellationToken = default);

    Task<bool> HasSystemKeyAsync(Guid userId, string systemKey, CancellationToken cancellationToken = default);

    Task<int> CountByUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<int> CountExpensesAsync(Guid categoryId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ExpenseCategoryListItemDto>> ListWithCountsAsync(
        Guid userId, CancellationToken cancellationToken = default);

    Task ReassignExpensesAsync(
        Guid fromCategoryId, Guid toCategoryId, CancellationToken cancellationToken = default);

    void Add(ExpenseCategory category);

    void Remove(ExpenseCategory category);
}
