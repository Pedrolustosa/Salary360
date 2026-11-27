using Salary360.Application.Expenses.Interfaces;
using Salary360.Application.Interfaces.Persistence;
using Salary360.Domain.Entities;
using Salary360.Domain.ExpenseCategories;
using Salary360.Domain.Interfaces;

namespace Salary360.Application.Expenses.Services;

public sealed class DefaultExpenseCategoryService(
    IExpenseCategoryRepository repository,
    IUnitOfWork unitOfWork) : IDefaultExpenseCategoryService
{
    public async Task<int> EnsureDefaultsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var created = 0;
        foreach (var (systemKey, color, icon) in DefaultExpenseCategories.All)
        {
            if (await repository.HasSystemKeyAsync(userId, systemKey, cancellationToken))
                continue;

            repository.Add(new ExpenseCategory(userId, null, systemKey, color, icon));
            created++;
        }

        if (created > 0)
            await unitOfWork.SaveChangesAsync(cancellationToken);

        return created;
    }
}
