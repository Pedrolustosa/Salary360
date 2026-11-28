using Salary360.Application.Common.Exceptions;
using Salary360.Application.Interfaces.Identity;
using Salary360.Application.Interfaces.Messaging;
using Salary360.Application.Interfaces.Persistence;
using Salary360.Domain.Entities;
using Salary360.Domain.Interfaces;
using Salary360.Application.Expenses.Dtos;

namespace Salary360.Application.Expenses.Categories.Commands;

public sealed class CreateExpenseCategoryCommandHandler(
    IExpenseCategoryRepository repository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser) : ICommandHandler<CreateExpenseCategoryCommand, ExpenseCategoryDto>
{
    public async Task<ExpenseCategoryDto> HandleAsync(
        CreateExpenseCategoryCommand request, CancellationToken cancellationToken = default)
    {
        var userId = currentUser.GetUserId();
        var name = request.Name.Trim();

        if (await repository.CountByUserAsync(userId, cancellationToken) >= ExpenseCategory.MaxPerUser)
            throw new ConflictException("Too many categories for this user.", "too_many_categories");

        if (await repository.ExistsWithNameAsync(userId, name, cancellationToken: cancellationToken))
            throw new ConflictException("A category with this name already exists.", "duplicate_category_name");

        var category = new ExpenseCategory(userId, name, null, request.ColorKey, request.IconKey);
        repository.Add(category);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return category.ToDto();
}
}