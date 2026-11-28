using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Salary360.Domain.Entities;
using Salary360.Domain.Enums;
using Salary360.Domain.ExpenseCategories;
using Salary360.Infrastructure.Events;
using Salary360.Infrastructure.Identity;
using Salary360.Infrastructure.Persistence;
using Salary360.Infrastructure.Seed;

namespace Salary360.UnitTests.Infrastructure;

public sealed class ExpensesSampleDataSeederTests : IAsyncLifetime
{
    private static readonly DateTimeOffset FixedUtc = new(2026, 11, 15, 12, 0, 0, TimeSpan.Zero);

    private readonly SqliteConnection connection = new("DataSource=:memory:");
    private AppDbContext context = null!;
    private Guid userId;
    private Guid adminId;

    public async Task InitializeAsync()
    {
        await connection.OpenAsync();
        var services = new ServiceCollection().BuildServiceProvider();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;
        context = new AppDbContext(options, new DomainEventDispatcher(services, NullLogger<DomainEventDispatcher>.Instance));
        await context.Database.EnsureCreatedAsync();

        userId = Guid.NewGuid();
        adminId = Guid.NewGuid();
        context.Users.Add(CreateUser(userId, "user@salary360.local", "Standard User"));
        context.Users.Add(CreateUser(adminId, "admin@salary360.local", "Administrator"));
        await context.SaveChangesAsync();

        await SeedSalariesAsync(userId, months: 24, isAdmin: false);
        await SeedSalariesAsync(adminId, months: 6, isAdmin: true);
    }

    public async Task DisposeAsync()
    {
        await context.DisposeAsync();
        await connection.DisposeAsync();
    }

    [Fact]
    public async Task SeedAsync_CreatesCategoriesAndExpensesForMediumVolume()
    {
        var seeder = CreateSeeder();

        await seeder.SeedAsync();

        var userCategories = await context.ExpenseCategories.CountAsync(category => category.UserId == userId);
        var adminCategories = await context.ExpenseCategories.CountAsync(category => category.UserId == adminId);
        var userExpenses = await context.Expenses.CountAsync(expense => expense.UserId == userId);
        var adminExpenses = await context.Expenses.CountAsync(expense => expense.UserId == adminId);

        Assert.Equal(DefaultExpenseCategories.All.Count, userCategories);
        Assert.Equal(DefaultExpenseCategories.All.Count, adminCategories);
        Assert.True(userExpenses > adminExpenses);
        Assert.True(userExpenses >= 24 * 3);
        Assert.True(adminExpenses >= 6);
    }

    [Fact]
    public async Task SeedAsync_IsIdempotent()
    {
        var seeder = CreateSeeder();

        await seeder.SeedAsync();
        var firstUser = await context.Expenses.CountAsync(expense => expense.UserId == userId);
        var firstAdmin = await context.Expenses.CountAsync(expense => expense.UserId == adminId);

        await seeder.SeedAsync();

        Assert.Equal(firstUser, await context.Expenses.CountAsync(expense => expense.UserId == userId));
        Assert.Equal(firstAdmin, await context.Expenses.CountAsync(expense => expense.UserId == adminId));
        Assert.Equal(
            DefaultExpenseCategories.All.Count,
            await context.ExpenseCategories.CountAsync(category => category.UserId == userId));
    }

    [Fact]
    public async Task SeedAsync_ProducesValidDomainRecordsWithoutFutureCompetences()
    {
        var seeder = CreateSeeder();

        await seeder.SeedAsync();

        var expenses = await context.Expenses
            .Where(expense => expense.UserId == userId)
            .ToListAsync();

        Assert.Contains(expenses, expense => expense.Amount == Expense.MaxAmount);
        Assert.Contains(expenses, expense => expense.Notes is { Length: Expense.MaxNotesLength });
        Assert.Contains(expenses, expense => expense.Type == ExpenseType.Fixed && expense.SeriesId != null);
        Assert.Contains(expenses, expense => expense.Type == ExpenseType.Variable);
        Assert.Contains(expenses, expense => expense.IsPaid);
        Assert.Contains(expenses, expense => !expense.IsPaid);
        Assert.All(expenses, expense =>
        {
            Assert.False(string.IsNullOrWhiteSpace(expense.Name));
            Assert.True(expense.Amount > 0);
            Assert.True(expense.Amount <= Expense.MaxAmount);
            Assert.InRange(expense.Year, 1900, 2100);
            Assert.InRange(expense.Month, 1, 12);
            Assert.True(
                expense.Year < FixedUtc.Year
                || (expense.Year == FixedUtc.Year && expense.Month <= FixedUtc.Month));
            if (expense.IsPaid)
                Assert.NotNull(expense.PaidAtUtc);
            else
                Assert.Null(expense.PaidAtUtc);
        });
    }

    [Fact]
    public async Task SeedAsync_CoversGapMonthAndOverNetMonth()
    {
        var seeder = CreateSeeder();

        await seeder.SeedAsync();

        var salaryKeys = await context.SalaryRecords
            .Where(record => record.UserId == userId)
            .Select(record => new { record.Year, record.Month })
            .ToListAsync();
        var salarySet = salaryKeys.Select(key => (key.Year, key.Month)).ToHashSet();

        var gap = await context.Expenses
            .Where(expense => expense.UserId == userId && expense.Name == "Gap month expense")
            .FirstOrDefaultAsync();
        Assert.NotNull(gap);
        Assert.DoesNotContain((gap.Year, gap.Month), salarySet);

        var overspend = await context.Expenses
            .Where(expense => expense.UserId == userId && expense.Name == "One-off overspend")
            .FirstOrDefaultAsync();
        Assert.NotNull(overspend);

        var salary = await context.SalaryRecords
            .Include(record => record.Items)
            .FirstAsync(record =>
                record.UserId == userId
                && record.Year == overspend.Year
                && record.Month == overspend.Month);

        var monthTotal = await context.Expenses
            .Where(expense =>
                expense.UserId == userId
                && expense.Year == overspend.Year
                && expense.Month == overspend.Month)
            .SumAsync(expense => expense.Amount);

        Assert.True(monthTotal > salary.NetAmount);
    }

    [Fact]
    public async Task SeedAsync_IsolatesUsers()
    {
        var seeder = CreateSeeder();

        await seeder.SeedAsync();

        var userNames = await context.Expenses
            .Where(expense => expense.UserId == userId)
            .Select(expense => expense.Name)
            .Distinct()
            .OrderBy(name => name)
            .ToListAsync();
        var adminNames = await context.Expenses
            .Where(expense => expense.UserId == adminId)
            .Select(expense => expense.Name)
            .Distinct()
            .OrderBy(name => name)
            .ToListAsync();

        Assert.NotEmpty(userNames);
        Assert.NotEmpty(adminNames);
        Assert.Contains("Education installment", userNames);
        Assert.DoesNotContain("Education installment", adminNames);
        Assert.Empty(await context.Expenses
            .Where(expense => expense.UserId == adminId && expense.Name == "Planning ceiling")
            .ToListAsync());
    }

    [Fact]
    public async Task SeedAsync_SkipsWhenSalariesMissing()
    {
        context.SalaryRecords.RemoveRange(context.SalaryRecords);
        await context.SaveChangesAsync();

        var seeder = CreateSeeder();
        await seeder.SeedAsync();

        Assert.Equal(0, await context.Expenses.CountAsync());
    }

    private async Task SeedSalariesAsync(Guid ownerId, int months, bool isAdmin)
    {
        var current = new DateOnly(FixedUtc.Year, FixedUtc.Month, 1);
        for (var offset = 0; offset < months; offset++)
        {
            var competence = current.AddMonths(-offset);
            if (offset == 3)
                continue;

            var gross = isAdmin ? 9000m : 5000m + offset * 10m;
            context.SalaryRecords.Add(new SalaryRecord(
                ownerId,
                competence.Year,
                competence.Month,
                gross,
                null,
                [("INSS", SalaryItemType.Deduction, 500m)]));
        }

        await context.SaveChangesAsync();
    }

    private ExpensesSampleDataSeeder CreateSeeder()
    {
        var settings = Options.Create(new SeedSettings
        {
            Admin = new SeedUserSettings { Email = "admin@salary360.local" },
            User = new SeedUserSettings { Email = "user@salary360.local" },
            SampleData = new SampleDataSeedSettings
            {
                Enabled = true,
                Volume = "medium"
            }
        });

        return new ExpensesSampleDataSeeder(
            context,
            settings,
            new FixedTimeProvider(FixedUtc),
            NullLogger<ExpensesSampleDataSeeder>.Instance);
    }

    private static ApplicationUser CreateUser(Guid id, string email, string displayName)
        => new()
        {
            Id = id,
            UserName = email,
            NormalizedUserName = email.ToUpperInvariant(),
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            EmailConfirmed = true,
            DisplayName = displayName,
            Status = UserStatus.Active,
            SecurityStamp = Guid.NewGuid().ToString("N"),
            ConcurrencyStamp = Guid.NewGuid().ToString("N"),
            CreatedAtUtc = FixedUtc.UtcDateTime
        };

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}