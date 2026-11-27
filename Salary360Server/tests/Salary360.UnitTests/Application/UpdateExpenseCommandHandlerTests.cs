using NSubstitute;
using Salary360.Application.Expenses.Commands;
using Salary360.Application.Interfaces.Identity;
using Salary360.Application.Interfaces.Persistence;
using Salary360.Domain.Entities;
using Salary360.Domain.Enums;
using Salary360.Domain.Interfaces;

namespace Salary360.UnitTests.Application;

public sealed class UpdateExpenseCommandHandlerTests
{
    private readonly IExpenseRepository expenseRepository = Substitute.For<IExpenseRepository>();
    private readonly IExpenseCategoryRepository categoryRepository = Substitute.For<IExpenseCategoryRepository>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUser currentUser = Substitute.For<ICurrentUser>();

    [Fact]
    public async Task HandleAsync_ScopeThis_UpdatesOnlyTarget()
    {
        var (userId, category, first, second) = BuildSeries();
        currentUser.UserId.Returns(userId);
        expenseRepository.GetByIdAsync(first.Id, Arg.Any<CancellationToken>()).Returns(first);
        categoryRepository.GetByIdAsync(category.Id, Arg.Any<CancellationToken>()).Returns(category);

        var handler = CreateHandler();
        await handler.HandleAsync(Command(first, category.Id, ExpenseChangeScope.This, amount: 111m));

        Assert.Equal(111m, first.Amount);
        Assert.Equal(100m, second.Amount);
        await expenseRepository.DidNotReceiveWithAnyArgs()
            .ListBySeriesAsync(default, default, default);
    }

    [Fact]
    public async Task HandleAsync_ScopeThisAndFollowing_UpdatesFromTarget()
    {
        var (userId, category, first, second) = BuildSeries();
        var third = new Expense(
            userId, category.Id, "Rent", ExpenseType.Fixed, 100m, 2024, 3, 5, false, null, first.SeriesId);
        currentUser.UserId.Returns(userId);
        expenseRepository.GetByIdAsync(second.Id, Arg.Any<CancellationToken>()).Returns(second);
        categoryRepository.GetByIdAsync(category.Id, Arg.Any<CancellationToken>()).Returns(category);
        expenseRepository.ListBySeriesFromAsync(userId, first.SeriesId!.Value, 2024, 2, Arg.Any<CancellationToken>())
            .Returns([second, third]);

        var handler = CreateHandler();
        await handler.HandleAsync(Command(second, category.Id, ExpenseChangeScope.ThisAndFollowing, amount: 222m));

        Assert.Equal(100m, first.Amount);
        Assert.Equal(222m, second.Amount);
        Assert.Equal(222m, third.Amount);
    }

    [Fact]
    public async Task HandleAsync_ScopeAll_UpdatesEntireSeries()
    {
        var (userId, category, first, second) = BuildSeries();
        currentUser.UserId.Returns(userId);
        expenseRepository.GetByIdAsync(second.Id, Arg.Any<CancellationToken>()).Returns(second);
        categoryRepository.GetByIdAsync(category.Id, Arg.Any<CancellationToken>()).Returns(category);
        expenseRepository.ListBySeriesAsync(userId, first.SeriesId!.Value, Arg.Any<CancellationToken>())
            .Returns([first, second]);

        var handler = CreateHandler();
        await handler.HandleAsync(Command(second, category.Id, ExpenseChangeScope.All, amount: 333m));

        Assert.Equal(333m, first.Amount);
        Assert.Equal(333m, second.Amount);
        Assert.Equal(2024, first.Year);
        Assert.Equal(1, first.Month);
        Assert.Equal(2024, second.Year);
        Assert.Equal(2, second.Month);
    }

    private UpdateExpenseCommandHandler CreateHandler()
        => new(expenseRepository, categoryRepository, unitOfWork, currentUser);

    private static (Guid UserId, ExpenseCategory Category, Expense First, Expense Second) BuildSeries()
    {
        var userId = Guid.NewGuid();
        var category = new ExpenseCategory(userId, "Housing", null, CategoryColor.Blue, CategoryIcon.Home);
        var seriesId = Guid.NewGuid();
        var first = new Expense(userId, category.Id, "Rent", ExpenseType.Fixed, 100m, 2024, 1, 5, false, null, seriesId);
        var second = new Expense(userId, category.Id, "Rent", ExpenseType.Fixed, 100m, 2024, 2, 5, false, null, seriesId);
        return (userId, category, first, second);
    }

    private static UpdateExpenseCommand Command(
        Expense expense,
        Guid categoryId,
        ExpenseChangeScope scope,
        decimal amount)
        => new(
            expense.Id,
            categoryId,
            expense.Name,
            expense.Type,
            amount,
            expense.Year,
            expense.Month,
            expense.DueDay,
            expense.IsPaid,
            expense.Notes,
            scope);
}
