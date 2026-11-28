using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Salary360.Domain.Common;
using Salary360.Domain.Entities;
using Salary360.Domain.Enums;
using Salary360.Domain.ExpenseCategories;
using Salary360.Infrastructure.Persistence;
using Salary360.Infrastructure.Seed.Interfaces;

namespace Salary360.Infrastructure.Seed;

public sealed class ExpensesSampleDataSeeder(
    AppDbContext dbContext,
    IOptions<SeedSettings> seedOptions,
    TimeProvider timeProvider,
    ILogger<ExpensesSampleDataSeeder> logger) : ISampleDataSeeder
{
    private const int UserRandomSeed = 360_101;
    private const int AdminRandomSeed = 360_102;
    private const string RentName = "Housing rent";
    private const string StreamingName = "Streaming bundle";
    private const string GroceriesName = "Groceries";
    private const string LeisureName = "Leisure outing";
    private const string TransportName = "Transport";
    private const string EducationName = "Education installment";
    private const string OverspendName = "One-off overspend";
    private const string GapMonthName = "Gap month expense";
    private const string PlanningName = "Planning ceiling";

    public int GetOrder() => 20;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var settings = seedOptions.Value;
        var volume = SampleDataVolumeCounts.Parse(settings.SampleData.Volume);
        var utcNow = timeProvider.GetUtcNow();
        var currentMonth = new DateOnly(utcNow.Year, utcNow.Month, 1);

        await SeedForEmailAsync(
            settings.User.Email,
            SampleDataVolumeCounts.UserExpenseMonthCount(volume),
            isAdminProfile: false,
            currentMonth,
            UserRandomSeed,
            cancellationToken);

        await SeedForEmailAsync(
            settings.Admin.Email,
            SampleDataVolumeCounts.AdminExpenseMonthCount(volume),
            isAdminProfile: true,
            currentMonth,
            AdminRandomSeed,
            cancellationToken);
    }

    private async Task SeedForEmailAsync(
        string email,
        int targetMonths,
        bool isAdminProfile,
        DateOnly currentMonth,
        int randomSeed,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            logger.LogWarning("Expense sample data skipped: seed user e-mail is not configured");
            return;
        }

        var user = await dbContext.Users.AsNoTracking()
            .FirstOrDefaultAsync(candidate => candidate.Email == email, cancellationToken);

        if (user is null)
        {
            logger.LogWarning("Expense sample data skipped: seed user was not found");
            return;
        }

        var categories = await EnsureDefaultCategoriesAsync(user.Id, cancellationToken);
        var salaries = await dbContext.SalaryRecords
            .AsNoTracking()
            .Include(record => record.Items)
            .Where(record => record.UserId == user.Id)
            .OrderBy(record => record.Year)
            .ThenBy(record => record.Month)
            .ToListAsync(cancellationToken);

        if (salaries.Count == 0)
        {
            logger.LogWarning("Expense sample data skipped: no salary records for seed user");
            return;
        }

        var monthCount = Math.Min(targetMonths, salaries.Count);
        var selected = salaries
            .TakeLast(monthCount)
            .ToList();

        var salaryKeys = salaries
            .Select(record => (record.Year, record.Month))
            .ToHashSet();

        var existing = await dbContext.Expenses.AsNoTracking()
            .Where(expense => expense.UserId == user.Id)
            .Select(expense => new { expense.Name, expense.CategoryId, expense.Year, expense.Month })
            .ToListAsync(cancellationToken);

        var existingKeys = existing
            .Select(row => (row.Name, row.CategoryId, row.Year, row.Month))
            .ToHashSet();

        var random = new Random(randomSeed);
        var rentSeriesId = Guid.Parse(isAdminProfile
            ? "a3600002-0001-4000-8000-000000000001"
            : "a3600001-0001-4000-8000-000000000001");
        var streamingSeriesId = Guid.Parse(isAdminProfile
            ? "a3600002-0002-4000-8000-000000000002"
            : "a3600001-0002-4000-8000-000000000002");
        var educationSeriesId = Guid.Parse(isAdminProfile
            ? "a3600002-0003-4000-8000-000000000003"
            : "a3600001-0003-4000-8000-000000000003");

        var created = 0;
        var skipped = 0;
        var overspendIndex = Math.Max(0, selected.Count / 2);

        foreach (var (index, salary) in selected.Select((record, index) => (index, record)))
        {
            var housingId = categories["housing"];
            var foodId = categories["food"];
            var leisureId = categories["leisure"];
            var transportId = categories["transport"];
            var subscriptionsId = categories["subscriptions"];

            var rentAmount = isAdminProfile ? 1800m : 2200m + (salary.Month % 3) * 25m;
            TryAdd(
                user.Id,
                housingId,
                RentName,
                ExpenseType.Fixed,
                rentAmount,
                salary.Year,
                salary.Month,
                5,
                isPaid: true,
                notes: null,
                rentSeriesId,
                existingKeys,
                ref created,
                ref skipped);

            var streamingAmount = isAdminProfile ? 49.90m : 59.90m + (salary.Year % 3);
            TryAdd(
                user.Id,
                subscriptionsId,
                StreamingName,
                ExpenseType.Fixed,
                streamingAmount,
                salary.Year,
                salary.Month,
                10,
                isPaid: index % 2 == 0,
                notes: null,
                streamingSeriesId,
                existingKeys,
                ref created,
                ref skipped);

            var groceries = Money.Round(450m + random.Next(20, 180) + salary.Month * 3m);
            TryAdd(
                user.Id,
                foodId,
                GroceriesName,
                ExpenseType.Variable,
                groceries,
                salary.Year,
                salary.Month,
                15,
                isPaid: index % 3 != 0,
                notes: null,
                null,
                existingKeys,
                ref created,
                ref skipped);

            if (!isAdminProfile || index % 2 == 0)
            {
                var leisure = Money.Round(80m + random.Next(10, 220));
                TryAdd(
                    user.Id,
                    leisureId,
                    LeisureName,
                    ExpenseType.Variable,
                    leisure,
                    salary.Year,
                    salary.Month,
                    20,
                    isPaid: false,
                    notes: null,
                    null,
                    existingKeys,
                    ref created,
                    ref skipped);

                var transport = Money.Round(120m + random.Next(0, 90) + (salary.Month % 4) * 15m);
                TryAdd(
                    user.Id,
                    transportId,
                    TransportName,
                    ExpenseType.Variable,
                    transport,
                    salary.Year,
                    salary.Month,
                    8,
                    isPaid: index % 4 != 1,
                    notes: null,
                    null,
                    existingKeys,
                    ref created,
                    ref skipped);
            }

            if (!isAdminProfile && index == overspendIndex)
            {
                var planned = rentAmount + streamingAmount + groceries + 300m;
                var overspend = Money.Round(Math.Max(salary.NetAmount - planned + 350m, 400m));
                TryAdd(
                    user.Id,
                    categories["other"],
                    OverspendName,
                    ExpenseType.Variable,
                    overspend,
                    salary.Year,
                    salary.Month,
                    28,
                    isPaid: false,
                    notes: "Sample month above net",
                    null,
                    existingKeys,
                    ref created,
                    ref skipped);
            }
        }

        if (!isAdminProfile)
        {
            var educationMonths = selected.TakeLast(Math.Min(6, selected.Count)).ToList();
            foreach (var salary in educationMonths)
            {
                TryAdd(
                    user.Id,
                    categories["education"],
                    EducationName,
                    ExpenseType.Fixed,
                    299.90m,
                    salary.Year,
                    salary.Month,
                    12,
                    isPaid: true,
                    notes: "Six-month course",
                    educationSeriesId,
                    existingKeys,
                    ref created,
                    ref skipped);
            }

            var gap = FindGapMonth(salaryKeys, selected, currentMonth);
            if (gap is not null)
            {
                var gapCompetence = new DateOnly(gap.Value.Year, gap.Value.Month, 1);
                if (gapCompetence <= currentMonth)
                {
                    TryAdd(
                        user.Id,
                        categories["other"],
                        GapMonthName,
                        ExpenseType.Variable,
                        175.50m,
                        gap.Value.Year,
                        gap.Value.Month,
                        18,
                        isPaid: false,
                        notes: "Competence without salary",
                        null,
                        existingKeys,
                        ref created,
                        ref skipped);
                }
            }

            var ceiling = selected[0];
            TryAdd(
                user.Id,
                categories["other"],
                PlanningName,
                ExpenseType.Fixed,
                Expense.MaxAmount,
                ceiling.Year,
                ceiling.Month,
                31,
                isPaid: false,
                notes: new string('E', Expense.MaxNotesLength),
                null,
                existingKeys,
                ref created,
                ref skipped);
        }

        if (created > 0)
            await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Expense sample data: {Created} created, {Skipped} skipped",
            created,
            skipped);
    }

    private async Task<Dictionary<string, Guid>> EnsureDefaultCategoriesAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var existing = await dbContext.ExpenseCategories
            .Where(category => category.UserId == userId && category.SystemKey != null)
            .ToListAsync(cancellationToken);

        var map = existing
            .Where(category => category.SystemKey is not null)
            .ToDictionary(category => category.SystemKey!, category => category.Id);

        var added = false;
        foreach (var (systemKey, color, icon) in DefaultExpenseCategories.All)
        {
            if (map.ContainsKey(systemKey))
                continue;

            var category = new ExpenseCategory(userId, null, systemKey, color, icon);
            dbContext.ExpenseCategories.Add(category);
            map[systemKey] = category.Id;
            added = true;
        }

        if (added)
            await dbContext.SaveChangesAsync(cancellationToken);

        return map;
    }

    private void TryAdd(
        Guid userId,
        Guid categoryId,
        string name,
        ExpenseType type,
        decimal amount,
        int year,
        int month,
        int? dueDay,
        bool isPaid,
        string? notes,
        Guid? seriesId,
        HashSet<(string Name, Guid CategoryId, int Year, int Month)> existingKeys,
        ref int created,
        ref int skipped)
    {
        var key = (name, categoryId, year, month);
        if (existingKeys.Contains(key))
        {
            skipped++;
            return;
        }

        dbContext.Expenses.Add(new Expense(
            userId,
            categoryId,
            name,
            type,
            amount,
            year,
            month,
            dueDay,
            isPaid,
            notes,
            seriesId,
            isPaid ? DateTime.UtcNow : null));

        existingKeys.Add(key);
        created++;
    }

    private static (int Year, int Month)? FindGapMonth(
        HashSet<(int Year, int Month)> salaryKeys,
        IReadOnlyList<SalaryRecord> selected,
        DateOnly currentMonth)
    {
        if (selected.Count == 0)
            return (currentMonth.Year, currentMonth.Month);

        var start = new DateOnly(selected[0].Year, selected[0].Month, 1);
        var end = new DateOnly(selected[^1].Year, selected[^1].Month, 1);

        for (var cursor = start; cursor <= end; cursor = cursor.AddMonths(1))
        {
            if (!salaryKeys.Contains((cursor.Year, cursor.Month)))
                return (cursor.Year, cursor.Month);
        }

        var after = end.AddMonths(1);
        if (!salaryKeys.Contains((after.Year, after.Month)))
            return (after.Year, after.Month);

        var before = start.AddMonths(-1);
        return (before.Year, before.Month);
    }
}