using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Salary360.Domain.Common;
using Salary360.Domain.Entities;
using Salary360.Domain.Enums;
using Salary360.Infrastructure.Events;
using Salary360.Infrastructure.Persistence;

namespace Salary360.UnitTests.Application;

public sealed class ExpenseRepositorySummaryTests : IAsyncLifetime
{
    private readonly SqliteConnection connection = new("DataSource=:memory:");
    private AppDbContext context = null!;
    private ExpenseRepository repository = null!;
    private readonly Guid userId = Guid.NewGuid();
    private Guid housingId;
    private Guid foodId;

    public async Task InitializeAsync()
    {
        await connection.OpenAsync();
        var services = new ServiceCollection().BuildServiceProvider();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;
        context = new AppDbContext(options, new DomainEventDispatcher(services, NullLogger<DomainEventDispatcher>.Instance));
        await context.Database.EnsureCreatedAsync();
        repository = new ExpenseRepository(context);

        var housing = new ExpenseCategory(userId, null, "housing", CategoryColor.Blue, CategoryIcon.Home);
        var food = new ExpenseCategory(userId, null, "food", CategoryColor.Orange, CategoryIcon.ShoppingBag);
        context.ExpenseCategories.AddRange(housing, food);
        await context.SaveChangesAsync();
        housingId = housing.Id;
        foodId = food.Id;
    }

    public async Task DisposeAsync()
    {
        await context.DisposeAsync();
        await connection.DisposeAsync();
    }

    [Fact]
    public async Task GetMonthSummary_TotalsPercentAndNegativeAvailable()
    {
        context.SalaryRecords.Add(new SalaryRecord(userId, 2024, 5, 3000m, null,
            [("VR", SalaryItemType.Benefit, 200m), ("INSS", SalaryItemType.Deduction, 400m)]));
        context.Expenses.Add(new Expense(userId, housingId, "Rent", ExpenseType.Fixed, 2000m, 2024, 5, 5, true, null, null));
        context.Expenses.Add(new Expense(userId, foodId, "Food", ExpenseType.Variable, 1250.125m, 2024, 5, 10, false, null, null));
        await context.SaveChangesAsync();

        var summary = await repository.GetMonthSummaryAsync(userId, 2024, 5);

        Assert.Equal(2000m, summary.TotalFixed);
        Assert.Equal(1250.12m, summary.TotalVariable);
        Assert.Equal(3250.12m, summary.Total);
        Assert.Equal(2000m, summary.TotalPaid);
        Assert.Equal(1250.12m, summary.TotalPending);
        Assert.Equal(2, summary.Count);
        Assert.True(summary.HasSalary);
        Assert.Equal(2800m, summary.NetAmount);
        Assert.Equal(Money.Round(3250.12m / 2800m * 100m), summary.CommittedPercent);
        Assert.Equal(Money.Round(2800m - 3250.12m), summary.Available);
        Assert.True(summary.Available < 0);

        var housingPercent = summary.ByCategory.Single(row => row.CategoryId == housingId).PercentOfTotal;
        var foodPercent = summary.ByCategory.Single(row => row.CategoryId == foodId).PercentOfTotal;
        Assert.Equal(Money.Round(2000m / 3250.12m * 100m), housingPercent);
        Assert.Equal(Money.Round(1250.12m / 3250.12m * 100m), foodPercent);
    }

    [Fact]
    public async Task GetMonthSummary_WithoutSalary_OmitsSalaryFields()
    {
        context.Expenses.Add(new Expense(userId, foodId, "Snack", ExpenseType.Variable, 40m, 2024, 6, null, false, null, null));
        await context.SaveChangesAsync();

        var summary = await repository.GetMonthSummaryAsync(userId, 2024, 6);

        Assert.Equal(40m, summary.Total);
        Assert.False(summary.HasSalary);
        Assert.Null(summary.NetAmount);
        Assert.Null(summary.CommittedPercent);
        Assert.Null(summary.Available);
    }

    [Fact]
    public async Task GetMonthSummary_PercentUsesBankersRounding()
    {
        context.Expenses.Add(new Expense(userId, housingId, "A", ExpenseType.Fixed, 1.235m, 2024, 7, null, true, null, null));
        context.Expenses.Add(new Expense(userId, foodId, "B", ExpenseType.Variable, 1.245m, 2024, 7, null, false, null, null));
        await context.SaveChangesAsync();

        var summary = await repository.GetMonthSummaryAsync(userId, 2024, 7);

        Assert.Equal(1.24m, summary.TotalFixed);
        Assert.Equal(1.24m, summary.TotalVariable);
        Assert.Equal(2.48m, summary.Total);
        Assert.Equal(Money.Round(1.24m / 2.48m * 100m), summary.ByCategory[0].PercentOfTotal);
    }
}