using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Salary360.Domain.Entities;
using Salary360.Domain.Enums;
using Salary360.Domain.Interfaces;
using Salary360.Infrastructure.Events;
using Salary360.Infrastructure.Persistence;

namespace Salary360.UnitTests.Application;

public sealed class SalaryRecordRepositoryEvolutionTests : IAsyncLifetime
{
    private readonly SqliteConnection connection = new("DataSource=:memory:");
    private AppDbContext context = null!;
    private SalaryRecordRepository repository = null!;
    private readonly Guid userId = Guid.NewGuid();

    public async Task InitializeAsync()
    {
        await connection.OpenAsync();
        var services = new ServiceCollection().BuildServiceProvider();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;
        context = new AppDbContext(options, new DomainEventDispatcher(services, NullLogger<DomainEventDispatcher>.Instance));
        await context.Database.EnsureCreatedAsync();
        repository = new SalaryRecordRepository(context);

        context.SalaryRecords.Add(new SalaryRecord(userId, 2022, 9, 3000m, null,
            [("B", SalaryItemType.Benefit, 100m)]));
        context.SalaryRecords.Add(new SalaryRecord(userId, 2024, 9, 4000m, null,
            [("D", SalaryItemType.Deduction, 200m)]));
        context.SalaryRecords.Add(new SalaryRecord(userId, 2024, 3, 5000m, null, []));
        await context.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await context.DisposeAsync();
        await connection.DisposeAsync();
    }

    [Fact]
    public async Task GetMonthEvolution_FillsGapsWithNull()
    {
        var points = await repository.GetMonthEvolutionAsync(userId, 9, 2022, 2024);

        Assert.Equal(3, points.Count);
        Assert.Equal(3100m, points[0].Net);
        Assert.Null(points[1].Net);
        Assert.Equal(3800m, points[2].Net);
    }

    [Fact]
    public async Task GetYearEvolution_AggregatesAndFillsGaps()
    {
        var points = await repository.GetYearEvolutionAsync(userId, 2022, 2024);

        Assert.Equal(3, points.Count);
        Assert.Equal(3100m, points[0].Net);
        Assert.Equal(1, points[0].MonthsCount);
        Assert.Null(points[1].Net);
        Assert.Equal(2, points[2].MonthsCount);
        Assert.Equal(8800m, points[2].Net);
        Assert.Equal(4400m, points[2].AverageNet);
    }
}
