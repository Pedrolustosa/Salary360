using NSubstitute;
using Salary360.Application.Expenses.Commands;
using Salary360.Application.Interfaces.Identity;
using Salary360.Application.Interfaces.Persistence;
using Salary360.Domain.Entities;
using Salary360.Domain.Enums;
using Salary360.Domain.Interfaces;

namespace Salary360.UnitTests.Application;

public sealed class DeleteExpenseCommandHandlerTests
{
    private readonly IExpenseRepository expenseRepository = Substitute.For<IExpenseRepository>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUser currentUser = Substitute.For<ICurrentUser>();

    [Fact]
    public async Task HandleAsync_ScopeThis_RemovesOnlyTarget()
    {
        var (userId, first, second) = BuildSeries();
        currentUser.GetUserId().Returns(userId);
        expenseRepository.GetByIdAsync(first.Id, Arg.Any<CancellationToken>()).Returns(first);

        var handler = new DeleteExpenseCommandHandler(expenseRepository, unitOfWork, currentUser);
        await handler.HandleAsync(new DeleteExpenseCommand(first.Id, ExpenseChangeScope.This));

        expenseRepository.Received(1).RemoveRange(Arg.Is<IEnumerable<Expense>>(items =>
            items.Single().Id == first.Id));
        await expenseRepository.DidNotReceiveWithAnyArgs()
            .ListBySeriesAsync(default, default, default);
    }

    [Fact]
    public async Task HandleAsync_ScopeThisAndFollowing_RemovesFromTarget()
    {
        var (userId, first, second) = BuildSeries();
        currentUser.GetUserId().Returns(userId);
        expenseRepository.GetByIdAsync(second.Id, Arg.Any<CancellationToken>()).Returns(second);
        expenseRepository.ListBySeriesFromAsync(userId, first.SeriesId!.Value, 2024, 2, Arg.Any<CancellationToken>())
            .Returns([second]);

        var handler = new DeleteExpenseCommandHandler(expenseRepository, unitOfWork, currentUser);
        await handler.HandleAsync(new DeleteExpenseCommand(second.Id, ExpenseChangeScope.ThisAndFollowing));

        expenseRepository.Received(1).RemoveRange(Arg.Is<IEnumerable<Expense>>(items =>
            items.Single().Id == second.Id));
    }

    [Fact]
    public async Task HandleAsync_ScopeAll_RemovesEntireSeries()
    {
        var (userId, first, second) = BuildSeries();
        currentUser.GetUserId().Returns(userId);
        expenseRepository.GetByIdAsync(second.Id, Arg.Any<CancellationToken>()).Returns(second);
        expenseRepository.ListBySeriesAsync(userId, first.SeriesId!.Value, Arg.Any<CancellationToken>())
            .Returns([first, second]);

        var handler = new DeleteExpenseCommandHandler(expenseRepository, unitOfWork, currentUser);
        await handler.HandleAsync(new DeleteExpenseCommand(second.Id, ExpenseChangeScope.All));

        expenseRepository.Received(1).RemoveRange(Arg.Is<IEnumerable<Expense>>(items =>
            items.Count() == 2));
    }

    private static (Guid UserId, Expense First, Expense Second) BuildSeries()
    {
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var seriesId = Guid.NewGuid();
        var first = new Expense(userId, categoryId, "Rent", ExpenseType.Fixed, 100m, 2024, 1, 5, false, null, seriesId);
        var second = new Expense(userId, categoryId, "Rent", ExpenseType.Fixed, 100m, 2024, 2, 5, false, null, seriesId);
        return (userId, first, second);
    }
}