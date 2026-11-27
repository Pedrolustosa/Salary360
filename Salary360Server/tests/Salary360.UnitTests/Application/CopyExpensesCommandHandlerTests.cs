using NSubstitute;
using Salary360.Application.Expenses.Commands;
using Salary360.Application.Interfaces.Identity;
using Salary360.Application.Interfaces.Persistence;
using Salary360.Domain.Entities;
using Salary360.Domain.Enums;
using Salary360.Domain.Interfaces;

namespace Salary360.UnitTests.Application;

public sealed class CopyExpensesCommandHandlerTests
{
    private readonly IExpenseRepository expenseRepository = Substitute.For<IExpenseRepository>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUser currentUser = Substitute.For<ICurrentUser>();

    [Fact]
    public async Task HandleAsync_SkipsDuplicates()
    {
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        currentUser.UserId.Returns(userId);

        var source = new List<Expense>
        {
            new(userId, categoryId, "Rent", ExpenseType.Fixed, 1000m, 2024, 1, 5, true, null, null),
            new(userId, categoryId, "Groceries", ExpenseType.Variable, 400m, 2024, 1, 10, false, null, null)
        };

        expenseRepository.ListForMonthAsync(userId, 2024, 1, false, Arg.Any<CancellationToken>())
            .Returns(source);
        expenseRepository.ExistingNameCategoryPairsAsync(userId, 2024, 2, Arg.Any<CancellationToken>())
            .Returns(new HashSet<(string Name, Guid CategoryId)> { ("Rent", categoryId) });
        expenseRepository.CountInMonthAsync(userId, 2024, 2, Arg.Any<CancellationToken>())
            .Returns(1);

        var handler = new CopyExpensesCommandHandler(expenseRepository, unitOfWork, currentUser);
        var result = await handler.HandleAsync(new CopyExpensesCommand(2024, 1, 2024, 2, OnlyFixed: false));

        Assert.Equal(1, result.Created);
        Assert.Equal(1, result.Skipped);
        expenseRepository.Received(1).AddRange(Arg.Is<IEnumerable<Expense>>(items =>
            items.Count() == 1 && items.Single().Name == "Groceries" && items.Single().IsPaid == false));
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
