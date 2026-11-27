using NSubstitute;
using Salary360.Application.Common.Exceptions;
using Salary360.Application.Expenses.Commands;
using Salary360.Application.Interfaces.Identity;
using Salary360.Application.Interfaces.Persistence;
using Salary360.Domain.Entities;
using Salary360.Domain.Enums;
using Salary360.Domain.Interfaces;

namespace Salary360.UnitTests.Application;

public sealed class CreateExpenseCommandHandlerTests
{
    private readonly IExpenseRepository expenseRepository = Substitute.For<IExpenseRepository>();
    private readonly IExpenseCategoryRepository categoryRepository = Substitute.For<IExpenseCategoryRepository>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUser currentUser = Substitute.For<ICurrentUser>();
    private readonly TimeProvider time = TimeProvider.System;

    [Fact]
    public async Task HandleAsync_MonthlyRecurrence_CreatesRequestedCount()
    {
        var userId = Guid.NewGuid();
        var category = new ExpenseCategory(userId, "Food", null, CategoryColor.Orange, CategoryIcon.ShoppingBag);
        currentUser.UserId.Returns(userId);
        categoryRepository.GetByIdAsync(category.Id, Arg.Any<CancellationToken>()).Returns(category);
        expenseRepository.CountInMonthAsync(userId, Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(0);

        var now = DateTime.UtcNow;
        var handler = CreateHandler();
        var result = await handler.HandleAsync(new CreateExpenseCommand(
            category.Id,
            "Groceries",
            ExpenseType.Variable,
            200m,
            now.Year,
            now.Month,
            10,
            true,
            null,
            ExpenseRecurrence.Monthly,
            3));

        Assert.Equal(3, result.Count);
        Assert.All(result, dto => Assert.NotNull(dto.SeriesId));
        Assert.Equal(result[0].SeriesId, result[1].SeriesId);
        Assert.True(result[0].IsPaid);
        Assert.False(result[1].IsPaid);
        Assert.False(result[2].IsPaid);
        expenseRepository.Received(1).AddRange(Arg.Is<IEnumerable<Expense>>(items => items.Count() == 3));
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_RecurrenceWrapsYear()
    {
        var userId = Guid.NewGuid();
        var category = new ExpenseCategory(userId, "Rent", null, CategoryColor.Blue, CategoryIcon.Home);
        currentUser.UserId.Returns(userId);
        categoryRepository.GetByIdAsync(category.Id, Arg.Any<CancellationToken>()).Returns(category);
        expenseRepository.CountInMonthAsync(userId, Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(0);

        var handler = CreateHandler();
        var result = await handler.HandleAsync(new CreateExpenseCommand(
            category.Id,
            "Rent",
            ExpenseType.Fixed,
            1500m,
            2024,
            11,
            5,
            false,
            null,
            ExpenseRecurrence.Monthly,
            3));

        Assert.Equal((2024, 11), (result[0].Year, result[0].Month));
        Assert.Equal((2024, 12), (result[1].Year, result[1].Month));
        Assert.Equal((2025, 1), (result[2].Year, result[2].Month));
    }

    [Fact]
    public async Task HandleAsync_MonthAtLimit_ThrowsConflict()
    {
        var userId = Guid.NewGuid();
        var category = new ExpenseCategory(userId, "Other", null, CategoryColor.Slate, CategoryIcon.EllipsisHorizontal);
        currentUser.UserId.Returns(userId);
        categoryRepository.GetByIdAsync(category.Id, Arg.Any<CancellationToken>()).Returns(category);
        var now = DateTime.UtcNow;
        expenseRepository.CountInMonthAsync(userId, now.Year, now.Month, Arg.Any<CancellationToken>())
            .Returns(Expense.MaxPerUserPerMonth);

        var handler = CreateHandler();
        var exception = await Assert.ThrowsAsync<ConflictException>(() => handler.HandleAsync(
            new CreateExpenseCommand(
                category.Id,
                "Overflow",
                ExpenseType.Variable,
                10m,
                now.Year,
                now.Month,
                null,
                false,
                null,
                ExpenseRecurrence.None,
                null)));

        Assert.Equal("too_many_expenses_in_month", exception.Code);
        expenseRepository.DidNotReceiveWithAnyArgs().AddRange(default!);
    }

    private CreateExpenseCommandHandler CreateHandler()
        => new(expenseRepository, categoryRepository, unitOfWork, currentUser, time);
}
