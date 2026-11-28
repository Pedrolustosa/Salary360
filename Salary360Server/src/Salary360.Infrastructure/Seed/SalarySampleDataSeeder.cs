using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Salary360.Domain.Common;
using Salary360.Domain.Entities;
using Salary360.Domain.Enums;
using Salary360.Infrastructure.Persistence;
using Salary360.Infrastructure.Seed.Interfaces;

namespace Salary360.Infrastructure.Seed;

public sealed class SalarySampleDataSeeder(
    AppDbContext dbContext,
    IOptions<SeedSettings> seedOptions,
    TimeProvider timeProvider,
    ILogger<SalarySampleDataSeeder> logger) : ISampleDataSeeder
{
    private const int UserRandomSeed = 360_001;
    private const int AdminRandomSeed = 360_002;

    public int GetOrder() => 10;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var settings = seedOptions.Value;
        var volume = SampleDataVolumeCounts.Parse(settings.SampleData.Volume);
        var utcNow = timeProvider.GetUtcNow();
        var currentMonth = new DateOnly(utcNow.Year, utcNow.Month, 1);

        await SeedForEmailAsync(
            settings.User.Email,
            SampleDataVolumeCounts.UserRecordCount(volume),
            isAdminProfile: false,
            currentMonth,
            UserRandomSeed,
            cancellationToken);

        await SeedForEmailAsync(
            settings.Admin.Email,
            SampleDataVolumeCounts.AdminRecordCount(volume),
            isAdminProfile: true,
            currentMonth,
            AdminRandomSeed,
            cancellationToken);
    }

    private async Task SeedForEmailAsync(
        string email,
        int targetCount,
        bool isAdminProfile,
        DateOnly currentMonth,
        int randomSeed,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            logger.LogWarning("Salary sample data skipped: seed user e-mail is not configured");
            return;
        }

        var user = await dbContext.Users.AsNoTracking()
            .FirstOrDefaultAsync(candidate => candidate.Email == email, cancellationToken);

        if (user is null)
        {
            logger.LogWarning("Salary sample data skipped: seed user was not found");
            return;
        }

        var existingKeys = await dbContext.SalaryRecords.AsNoTracking()
            .Where(record => record.UserId == user.Id)
            .Select(record => new { record.Year, record.Month })
            .ToListAsync(cancellationToken);

        var existing = existingKeys
            .Select(key => (key.Year, key.Month))
            .ToHashSet();

        var competences = BuildCompetences(targetCount, currentMonth, randomSeed);
        var ordered = competences
            .OrderBy(competence => competence.Year)
            .ThenBy(competence => competence.Month)
            .ToList();

        var created = 0;
        var skipped = 0;
        var random = new Random(randomSeed);
        var careerStartYear = ordered[0].Year;

        for (var index = 0; index < ordered.Count; index++)
        {
            var (year, month) = ordered[index];
            if (existing.Contains((year, month)))
            {
                skipped++;
                continue;
            }

            var record = BuildRecord(
                user.Id,
                year,
                month,
                index,
                ordered.Count,
                careerStartYear,
                isAdminProfile,
                random);
            dbContext.SalaryRecords.Add(record);
            created++;
        }

        if (created > 0)
            await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Salary sample data: {Created} created, {Skipped} skipped",
            created,
            skipped);
    }

    private static List<(int Year, int Month)> BuildCompetences(int targetCount, DateOnly currentMonth, int randomSeed)
    {
        var gapCount = targetCount >= 12 ? Math.Max(2, targetCount / 8) : Math.Max(0, targetCount / 6);
        var spanHint = targetCount + gapCount + 2;
        var random = new Random(randomSeed);
        var gaps = new HashSet<int>();

        while (gaps.Count < gapCount)
        {
            var yearBlock = random.Next(0, Math.Max(1, spanHint / 12));
            var monthInYear = random.Next(1, 12);
            var offset = yearBlock * 12 + monthInYear;
            if (offset > 0)
                gaps.Add(offset);
        }

        if (targetCount >= 24)
        {
            gaps.Add(14);
            gaps.Add(15);
        }

        var competences = new List<(int Year, int Month)>(targetCount);
        for (var offset = 0; competences.Count < targetCount; offset++)
        {
            if (gaps.Contains(offset))
                continue;

            var competence = currentMonth.AddMonths(-offset);
            competences.Add((competence.Year, competence.Month));
        }

        return competences;
    }

    private static SalaryRecord BuildRecord(
        Guid userId,
        int year,
        int month,
        int indexFromOldest,
        int totalCount,
        int careerStartYear,
        bool isAdminProfile,
        Random random)
    {
        var yearsOfCareer = year - careerStartYear;
        var gross = BuildGross(year, month, yearsOfCareer, indexFromOldest, totalCount, isAdminProfile);
        var notes = BuildNotes(year, month, indexFromOldest, totalCount);
        var items = BuildItems(year, month, indexFromOldest, totalCount, gross, isAdminProfile, random);
        return new SalaryRecord(userId, year, month, gross, notes, items);
    }

    private static decimal BuildGross(
        int year,
        int month,
        int yearsOfCareer,
        int indexFromOldest,
        int totalCount,
        bool isAdminProfile)
    {
        if (isAdminProfile)
        {
            var adminBase = 8500m + (yearsOfCareer * 450m);
            if (month == 1)
                return adminBase + 200m;
            return adminBase + (indexFromOldest % 3) * 40m;
        }

        var baseGross = 4200m + (yearsOfCareer * 520m);
        if (yearsOfCareer >= 2 && month >= 7)
            baseGross += 350m;

        if (month == 3)
            baseGross += 180m;

        var peakIndex = Math.Max(0, totalCount * 2 / 3);
        if (indexFromOldest == peakIndex)
            return baseGross + 2200m;

        var stableStart = Math.Max(0, totalCount - 5);
        if (indexFromOldest >= stableStart)
            return 4200m + (yearsOfCareer * 520m);

        return baseGross + (month % 4) * 25m;
    }

    private static string? BuildNotes(int year, int month, int indexFromOldest, int totalCount)
    {
        if (indexFromOldest == 0)
            return null;

        if (indexFromOldest == Math.Max(1, totalCount / 2))
            return new string('N', SalaryRecord.MaxNotesLength);

        if (month == 1)
            return $"Annual raise applied for {year}";

        if (month == 12)
            return $"Year-end settlement {year}";

        if (month == 3)
            return $"Profit sharing installment {year}";

        if (indexFromOldest % 11 == 0)
            return "Role adjustment sample note";

        return null;
    }

    private static List<(string Name, SalaryItemType Type, decimal Amount)> BuildItems(
        int year,
        int month,
        int indexFromOldest,
        int totalCount,
        decimal gross,
        bool isAdminProfile,
        Random random)
    {
        if (indexFromOldest == 1 && totalCount > 1)
            return [];

        var negativeNetIndex = Math.Max(2, totalCount / 4);
        if (!isAdminProfile && indexFromOldest == negativeNetIndex)
        {
            return
            [
                ("Meal Voucher", SalaryItemType.Benefit, 200m),
                ("Advance Recovery", SalaryItemType.Deduction, gross + 400m),
                ("INSS", SalaryItemType.Deduction, 500m)
            ];
        }

        if (indexFromOldest == Math.Max(3, totalCount - 2))
        {
            return
            [
                ("Meal Voucher", SalaryItemType.Benefit, 750m + random.Next(0, 50)),
                ("Health Allowance", SalaryItemType.Benefit, 420m),
                ("Home Office", SalaryItemType.Benefit, 200m),
                ("Performance Bonus", SalaryItemType.Benefit, 900m),
                ("Transport", SalaryItemType.Benefit, 220m),
                ("INSS", SalaryItemType.Deduction, Money.Round(gross * 0.11m)),
                ("IRRF", SalaryItemType.Deduction, Money.Round(gross * 0.15m)),
                ("Pension", SalaryItemType.Deduction, 350m),
                ("Union Fee", SalaryItemType.Deduction, 45m)
            ];
        }

        var meal = isAdminProfile ? 600m + (year % 3) * 20m : 650m + (year % 5) * 30m + (month % 3) * 10m;
        var health = isAdminProfile ? 320m : 360m + (year % 4) * 25m;
        var inss = Money.Round(gross * 0.11m);
        var irrfRate = isAdminProfile ? 0.125m : 0.135m + Math.Min(yearsFactor(year, indexFromOldest), 0.03m);
        var irrf = Money.Round(gross * irrfRate);

        var items = new List<(string Name, SalaryItemType Type, decimal Amount)>
        {
            ("Meal Voucher", SalaryItemType.Benefit, meal),
            ("INSS", SalaryItemType.Deduction, Math.Max(0.01m, inss)),
            ("IRRF", SalaryItemType.Deduction, Math.Max(0.01m, irrf))
        };

        if (month is 3 or 9 || indexFromOldest % 3 == 0)
            items.Insert(1, ("Health Allowance", SalaryItemType.Benefit, health));

        if (month == 12)
            items.Insert(1, ("Thirteenth Salary", SalaryItemType.Benefit, Money.Round(gross * 0.92m)));

        if (month == 3 && !isAdminProfile)
            items.Insert(1, ("Profit Sharing", SalaryItemType.Benefit, 1200m + (year % 4) * 150m));

        if (month is 1 or 7 || indexFromOldest % 4 == 0)
            items.Add(("Transport Deduction", SalaryItemType.Deduction, 110m + (month % 9) * 5m));

        if (month == 11 || indexFromOldest % 6 == 0)
            items.Insert(items.Count > 1 ? 1 : 0, ("Performance Bonus", SalaryItemType.Benefit, 450m + random.Next(0, 250)));

        if (year % 2 == 0 && month == 6)
            items.Insert(1, ("Education Aid", SalaryItemType.Benefit, 280m));

        return items;

        static decimal yearsFactor(int year, int index) => ((year % 5) + (index % 3)) * 0.005m;
    }
}