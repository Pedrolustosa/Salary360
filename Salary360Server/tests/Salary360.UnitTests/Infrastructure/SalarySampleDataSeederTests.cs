using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Salary360.Domain.Enums;
using Salary360.Infrastructure.Events;
using Salary360.Infrastructure.Identity;
using Salary360.Infrastructure.Persistence;
using Salary360.Infrastructure.Seed;

namespace Salary360.UnitTests.Infrastructure;

public sealed class SalarySampleDataSeederTests : IAsyncLifetime
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
    }

    public async Task DisposeAsync()
    {
        await context.DisposeAsync();
        await connection.DisposeAsync();
    }

    [Fact]
    public async Task SeedAsync_CreatesExpectedCountsForMediumVolume()
    {
        var seeder = CreateSeeder(enabled: true, volume: "medium");

        await seeder.SeedAsync();

        var userCount = await context.SalaryRecords.CountAsync(record => record.UserId == userId);
        var adminCount = await context.SalaryRecords.CountAsync(record => record.UserId == adminId);

        Assert.Equal(48, userCount);
        Assert.Equal(18, adminCount);
    }

    [Fact]
    public async Task SeedAsync_IsIdempotent()
    {
        var seeder = CreateSeeder(enabled: true, volume: "medium");

        await seeder.SeedAsync();
        await seeder.SeedAsync();

        var userCount = await context.SalaryRecords.CountAsync(record => record.UserId == userId);
        var adminCount = await context.SalaryRecords.CountAsync(record => record.UserId == adminId);

        Assert.Equal(48, userCount);
        Assert.Equal(18, adminCount);
    }

    [Fact]
    public async Task SeedAsync_SpansMultipleYearsForMediumVolume()
    {
        var seeder = CreateSeeder(enabled: true, volume: "medium");

        await seeder.SeedAsync();

        var userYears = await context.SalaryRecords
            .Where(record => record.UserId == userId)
            .Select(record => record.Year)
            .Distinct()
            .OrderBy(year => year)
            .ToListAsync();
        var adminYears = await context.SalaryRecords
            .Where(record => record.UserId == adminId)
            .Select(record => record.Year)
            .Distinct()
            .OrderBy(year => year)
            .ToListAsync();

        Assert.True(userYears.Count >= 4, $"Expected at least 4 years for user, got {userYears.Count}: {string.Join(',', userYears)}");
        Assert.True(adminYears.Count >= 2, $"Expected at least 2 years for admin, got {adminYears.Count}: {string.Join(',', adminYears)}");

        var userRecords = await context.SalaryRecords
            .Include(record => record.Items)
            .Where(record => record.UserId == userId)
            .ToListAsync();
        Assert.Contains(userRecords, record =>
            record.Month == 12 && record.Items.Any(item => item.Name == "Thirteenth Salary"));
    }

    [Fact]
    public async Task SeedAsync_ProducesValidDomainRecords()
    {
        var seeder = CreateSeeder(enabled: true, volume: "small");

        await seeder.SeedAsync();

        var records = await context.SalaryRecords
            .Include(record => record.Items)
            .Where(record => record.UserId == userId)
            .ToListAsync();

        Assert.Equal(12, records.Count);
        Assert.Contains(records, record => record.NetAmount < 0);
        Assert.Contains(records, record => record.Items.Count == 0);
        Assert.Contains(records, record => record.Notes is { Length: Salary360.Domain.Entities.SalaryRecord.MaxNotesLength });
        Assert.True(records.Select(record => record.Year).Distinct().Count() >= 2);
        Assert.All(records, record =>
        {
            Assert.True(record.GrossAmount > 0);
            Assert.InRange(record.Year, 1900, 2100);
            Assert.InRange(record.Month, 1, 12);
            Assert.True(record.Year < FixedUtc.Year
                        || (record.Year == FixedUtc.Year && record.Month <= FixedUtc.Month));
            Assert.True(record.Items.Count <= Salary360.Domain.Entities.SalaryRecord.MaxItems);
            Assert.All(record.Items, item =>
            {
                Assert.False(string.IsNullOrWhiteSpace(item.Name));
                Assert.True(item.Amount > 0);
                Assert.True(Enum.IsDefined(item.Type));
            });
            Assert.Equal(
                record.Items.Select(item => item.Name.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).Count(),
                record.Items.Count);
        });
    }

    [Fact]
    public async Task SeedAsync_IsolatesUsers()
    {
        var seeder = CreateSeeder(enabled: true, volume: "small");

        await seeder.SeedAsync();

        var userYears = await context.SalaryRecords
            .Where(record => record.UserId == userId)
            .Select(record => record.GrossAmount)
            .OrderBy(amount => amount)
            .ToListAsync();
        var adminYears = await context.SalaryRecords
            .Where(record => record.UserId == adminId)
            .Select(record => record.GrossAmount)
            .OrderBy(amount => amount)
            .ToListAsync();

        Assert.NotEmpty(userYears);
        Assert.NotEmpty(adminYears);
        Assert.NotEqual(userYears, adminYears);
    }

    [Fact]
    public async Task SeedAsync_SkipsWhenUserMissing()
    {
        context.Users.RemoveRange(context.Users);
        await context.SaveChangesAsync();

        var seeder = CreateSeeder(enabled: true, volume: "small");
        await seeder.SeedAsync();

        Assert.Equal(0, await context.SalaryRecords.CountAsync());
    }

    private SalarySampleDataSeeder CreateSeeder(bool enabled, string volume)
    {
        var settings = Options.Create(new SeedSettings
        {
            Admin = new SeedUserSettings { Email = "admin@salary360.local" },
            User = new SeedUserSettings { Email = "user@salary360.local" },
            SampleData = new SampleDataSeedSettings
            {
                Enabled = enabled,
                Volume = volume
            }
        });

        return new SalarySampleDataSeeder(
            context,
            settings,
            new FixedTimeProvider(FixedUtc),
            NullLogger<SalarySampleDataSeeder>.Instance);
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
