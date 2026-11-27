using Salary360.Application.Common;
using Salary360.Application.Common.Exceptions;
using Salary360.Application.Interfaces.Identity;
using Salary360.Application.Interfaces.Messaging;
using Salary360.Application.Interfaces.Persistence;
using Salary360.Domain.Interfaces;

namespace Salary360.Application.Expenses.Categories.Commands;

public sealed class DeleteExpenseCategoryCommandHandler(
    IExpenseCategoryRepository repository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser) : ICommandHandler<DeleteExpenseCategoryCommand, Unit>
{
    public async Task<Unit> HandleAsync(DeleteExpenseCategoryCommand request, CancellationToken cancellationToken = default)
    {
        var category = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (category is null || category.UserId != currentUser.UserId)
            throw new NotFoundException("ExpenseCategory", request.Id, "category_not_found");

        var expenseCount = await repository.CountExpensesAsync(category.Id, cancellationToken);
        if (expenseCount > 0)
        {
            if (request.ReassignToCategoryId is null)
                throw new ConflictException("Category has expenses and cannot be deleted.", "category_in_use");

            var target = await repository.GetByIdAsync(request.ReassignToCategoryId.Value, cancellationToken);
            if (target is null || target.UserId != currentUser.UserId)
                throw new NotFoundException("ExpenseCategory", request.ReassignToCategoryId.Value, "category_not_found");

            if (target.Id == category.Id)
                throw new ConflictException("Cannot reassign a category to itself.", "category_in_use");

            await repository.ReassignExpensesAsync(category.Id, target.Id, cancellationToken);
        }

        repository.Remove(category);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
