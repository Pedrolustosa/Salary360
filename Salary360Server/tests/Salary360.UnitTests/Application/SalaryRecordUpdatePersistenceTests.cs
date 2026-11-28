using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Salary360.Domain.Entities;
using Salary360.Domain.Enums;
using Salary360.Infrastructure.Events;
using Salary360.Infrastructure.Persistence;

namespace Salary360.UnitTests.Application;

public sealed class SalaryRecordUpdatePersistenceTests
{
    [Fact]
    public async Task Update_ReplacingItems_PersistsWithoutConcurrencyError()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var services = new ServiceCollection().BuildServiceProvider();
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options;
        await using var context = new AppDbContext(
            options, new DomainEventDispatcher(services, NullLogger<DomainEventDispatcher>.Instance));
        await context.Database.EnsureCreatedAsync();

        var record = new SalaryRecord(
            Guid.NewGuid(), 2024, 3, 5000m, null,
            [("VR", SalaryItemType.Benefit, 400m), ("INSS", SalaryItemType.Deduction, 500m)]);
        context.SalaryRecords.Add(record);
        await context.SaveChangesAsync();

        var loaded = await context.SalaryRecords.Include(r => r.Items).SingleAsync(r => r.Id == record.Id);
        loaded.Update(2024, 3, 6000m, "U", [("Bonus", SalaryItemType.Benefit, 100m)]);
        var rows = await context.SaveChangesAsync();

        Assert.True(rows > 0);
        var reloaded = await context.SalaryRecords.AsNoTracking().Include(r => r.Items).SingleAsync(r => r.Id == record.Id);
        Assert.Equal(6100m, reloaded.NetAmount);
        Assert.Single(reloaded.Items);
        Assert.Equal("Bonus", reloaded.Items.First().Name);
    }
}