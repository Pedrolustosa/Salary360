namespace Salary360.Application.Expenses.Interfaces;

public interface IDefaultExpenseCategoryService
{
    Task<int> EnsureDefaultsAsync(Guid userId, CancellationToken cancellationToken = default);
}
