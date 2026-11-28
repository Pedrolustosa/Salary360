using Salary360.Application.Common.Exceptions;
using Salary360.Application.Interfaces.Identity;
using Salary360.Application.Interfaces.Messaging;
using Salary360.Application.Interfaces.Persistence;
using Salary360.Domain.Interfaces;
using Salary360.Application.Expenses.Dtos;

namespace Salary360.Application.Expenses.Categories.Commands;

public sealed class UpdateExpenseCategoryCommandHandler(
    IExpenseCategoryRepository repository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser) : ICommandHandler<UpdateExpenseCategoryCommand, ExpenseCategoryDto>
{
    public async Task<ExpenseCategoryDto> HandleAsync(
        UpdateExpenseCategoryCommand request, CancellationToken cancellationToken = default)
    {
        var category = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (category is null || category.UserId != currentUser.GetUserId())
            throw new NotFoundException("ExpenseCategory", request.Id, "category_not_found");

        var name = string.IsNullOrWhiteSpace(request.Name) ? null : request.Name.Trim();
        if (name is not null
            && await repository.ExistsWithNameAsync(currentUser.GetUserId(), name, request.Id, cancellationToken))
            throw new ConflictException("A category with this name already exists.", "duplicate_category_name");

        category.Update(name, request.ColorKey, request.IconKey);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return category.ToDto();
}
}