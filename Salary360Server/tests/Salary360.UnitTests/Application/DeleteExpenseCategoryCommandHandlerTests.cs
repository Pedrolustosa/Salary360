using NSubstitute;
using Salary360.Application.Common.Exceptions;
using Salary360.Application.Expenses.Categories.Commands;
using Salary360.Application.Interfaces.Identity;
using Salary360.Application.Interfaces.Persistence;
using Salary360.Domain.Entities;
using Salary360.Domain.Enums;
using Salary360.Domain.Interfaces;

namespace Salary360.UnitTests.Application;

public sealed class DeleteExpenseCategoryCommandHandlerTests
{
    private readonly IExpenseCategoryRepository repository = Substitute.For<IExpenseCategoryRepository>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUser currentUser = Substitute.For<ICurrentUser>();

    [Fact]
    public async Task HandleAsync_InUseWithoutReassign_ThrowsConflict()
    {
        var userId = Guid.NewGuid();
        var category = new ExpenseCategory(userId, "Food", null, CategoryColor.Orange, CategoryIcon.ShoppingBag);
        currentUser.UserId.Returns(userId);
        repository.GetByIdAsync(category.Id, Arg.Any<CancellationToken>()).Returns(category);
        repository.CountExpensesAsync(category.Id, Arg.Any<CancellationToken>()).Returns(2);

        var handler = new DeleteExpenseCategoryCommandHandler(repository, unitOfWork, currentUser);
        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            handler.HandleAsync(new DeleteExpenseCategoryCommand(category.Id, null)));

        Assert.Equal("category_in_use", exception.Code);
        repository.DidNotReceiveWithAnyArgs().Remove(default!);
    }

    [Fact]
    public async Task HandleAsync_InUseWithReassign_ReassignsAndDeletes()
    {
        var userId = Guid.NewGuid();
        var source = new ExpenseCategory(userId, "Food", null, CategoryColor.Orange, CategoryIcon.ShoppingBag);
        var target = new ExpenseCategory(userId, "Other", null, CategoryColor.Slate, CategoryIcon.EllipsisHorizontal);
        currentUser.UserId.Returns(userId);
        repository.GetByIdAsync(source.Id, Arg.Any<CancellationToken>()).Returns(source);
        repository.GetByIdAsync(target.Id, Arg.Any<CancellationToken>()).Returns(target);
        repository.CountExpensesAsync(source.Id, Arg.Any<CancellationToken>()).Returns(1);

        var handler = new DeleteExpenseCategoryCommandHandler(repository, unitOfWork, currentUser);
        await handler.HandleAsync(new DeleteExpenseCategoryCommand(source.Id, target.Id));

        await repository.Received(1).ReassignExpensesAsync(source.Id, target.Id, Arg.Any<CancellationToken>());
        repository.Received(1).Remove(source);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
