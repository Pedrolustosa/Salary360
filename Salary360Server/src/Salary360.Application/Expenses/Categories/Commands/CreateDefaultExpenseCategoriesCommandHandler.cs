using Salary360.Application.Expenses.Interfaces;
using Salary360.Application.Interfaces.Identity;
using Salary360.Application.Interfaces.Messaging;
using Salary360.Application.Expenses.Dtos;

namespace Salary360.Application.Expenses.Categories.Commands;

public sealed class CreateDefaultExpenseCategoriesCommandHandler(
    IDefaultExpenseCategoryService defaults,
    ICurrentUser currentUser) : ICommandHandler<CreateDefaultExpenseCategoriesCommand, CreateDefaultsResultDto>
{
    public async Task<CreateDefaultsResultDto> HandleAsync(
        CreateDefaultExpenseCategoriesCommand request, CancellationToken cancellationToken = default)
    {
        var created = await defaults.EnsureDefaultsAsync(currentUser.GetUserId(), cancellationToken);
        return new CreateDefaultsResultDto(created);
}
}