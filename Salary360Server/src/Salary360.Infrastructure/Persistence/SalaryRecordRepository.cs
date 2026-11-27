using Microsoft.EntityFrameworkCore;
using Salary360.Application.Interfaces.Persistence;
using Salary360.Application.Salaries;
using Salary360.Domain.Common;
using Salary360.Domain.Entities;
using Salary360.Domain.Enums;

namespace Salary360.Infrastructure.Persistence;

public sealed class SalaryRecordRepository(AppDbContext context) : ISalaryRecordRepository
{
    public Task<SalaryRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => context.SalaryRecords
            .Include(record => record.Items)
            .FirstOrDefaultAsync(record => record.Id == id, cancellationToken);

    public Task<SalaryRecord?> GetByCompetenceAsync(
        Guid userId, int year, int month, CancellationToken cancellationToken = default)
        => context.SalaryRecords
            .Include(record => record.Items)
            .FirstOrDefaultAsync(
                record => record.UserId == userId && record.Year == year && record.Month == month,
                cancellationToken);

    public Task<bool> ExistsAsync(
        Guid userId, int year, int month, Guid? excludingId = null, CancellationToken cancellationToken = default)
        => context.SalaryRecords.AnyAsync(
            record => record.UserId == userId
                      && record.Year == year
                      && record.Month == month
                      && (excludingId == null || record.Id != excludingId),
            cancellationToken);

    public async Task<(IReadOnlyList<SalaryRecord> Items, int Total)> ListAsync(
        Guid userId, int? year, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = context.SalaryRecords
            .AsNoTracking()
            .Include(record => record.Items)
            .Where(record => record.UserId == userId);

        if (year.HasValue)
            query = query.Where(record => record.Year == year.Value);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(record => record.Year)
            .ThenByDescending(record => record.Month)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<IReadOnlyList<SalaryEvolutionPointDto>> GetMonthEvolutionAsync(
        Guid userId, int month, int fromYear, int toYear, CancellationToken cancellationToken = default)
    {
        var rows = await context.SalaryRecords
            .AsNoTracking()
            .Where(record =>
                record.UserId == userId
                && record.Month == month
                && record.Year >= fromYear
                && record.Year <= toYear)
            .Select(record => new
            {
                record.Year,
                record.GrossAmount,
                Benefits = record.Items
                    .Where(item => item.Type == SalaryItemType.Benefit)
                    .Sum(item => item.Amount),
                Deductions = record.Items
                    .Where(item => item.Type == SalaryItemType.Deduction)
                    .Sum(item => item.Amount)
            })
            .ToListAsync(cancellationToken);

        var byYear = rows.ToDictionary(row => row.Year);
        var points = new List<SalaryEvolutionPointDto>(toYear - fromYear + 1);

        for (var year = fromYear; year <= toYear; year++)
        {
            if (!byYear.TryGetValue(year, out var row))
            {
                points.Add(new SalaryEvolutionPointDto(year, null, null, null, null));
                continue;
            }

            var benefits = Money.Round(row.Benefits);
            var deductions = Money.Round(row.Deductions);
            var gross = Money.Round(row.GrossAmount);
            points.Add(new SalaryEvolutionPointDto(
                year,
                gross,
                benefits,
                deductions,
                Money.Round(gross + benefits - deductions)));
        }

        return points;
    }

    public async Task<IReadOnlyList<SalaryEvolutionPointDto>> GetYearEvolutionAsync(
        Guid userId, int fromYear, int toYear, CancellationToken cancellationToken = default)
    {
        var rows = await context.SalaryRecords
            .AsNoTracking()
            .Where(record =>
                record.UserId == userId
                && record.Year >= fromYear
                && record.Year <= toYear)
            .GroupBy(record => record.Year)
            .Select(group => new
            {
                Year = group.Key,
                Gross = group.Sum(record => record.GrossAmount),
                Benefits = group.SelectMany(record => record.Items)
                    .Where(item => item.Type == SalaryItemType.Benefit)
                    .Sum(item => item.Amount),
                Deductions = group.SelectMany(record => record.Items)
                    .Where(item => item.Type == SalaryItemType.Deduction)
                    .Sum(item => item.Amount),
                MonthsCount = group.Count(),
                NetSum = group.Sum(record =>
                    record.GrossAmount
                    + record.Items.Where(item => item.Type == SalaryItemType.Benefit).Sum(item => item.Amount)
                    - record.Items.Where(item => item.Type == SalaryItemType.Deduction).Sum(item => item.Amount))
            })
            .ToListAsync(cancellationToken);

        var byYear = rows.ToDictionary(row => row.Year);
        var points = new List<SalaryEvolutionPointDto>(toYear - fromYear + 1);

        for (var year = fromYear; year <= toYear; year++)
        {
            if (!byYear.TryGetValue(year, out var row))
            {
                points.Add(new SalaryEvolutionPointDto(year, null, null, null, null));
                continue;
            }

            var gross = Money.Round(row.Gross);
            var benefits = Money.Round(row.Benefits);
            var deductions = Money.Round(row.Deductions);
            var net = Money.Round(row.NetSum);
            points.Add(new SalaryEvolutionPointDto(
                year,
                gross,
                benefits,
                deductions,
                net,
                Money.Round(net / row.MonthsCount),
                row.MonthsCount));
        }

        return points;
    }

    public async Task<(int Year, int Month)?> GetLatestCompetenceAsync(
        Guid userId, CancellationToken cancellationToken = default)
    {
        var latest = await context.SalaryRecords
            .AsNoTracking()
            .Where(record => record.UserId == userId)
            .OrderByDescending(record => record.Year)
            .ThenByDescending(record => record.Month)
            .Select(record => new { record.Year, record.Month })
            .FirstOrDefaultAsync(cancellationToken);

        return latest is null ? null : (latest.Year, latest.Month);
    }

    public async Task<IReadOnlyList<SalaryRecord>> ListByPeriodAsync(
        Guid userId,
        int fromYear,
        int fromMonth,
        int toYear,
        int toMonth,
        CancellationToken cancellationToken = default)
    {
        var fromKey = fromYear * 12 + fromMonth;
        var toKey = toYear * 12 + toMonth;

        return await context.SalaryRecords
            .AsNoTracking()
            .Include(record => record.Items)
            .Where(record =>
                record.UserId == userId
                && record.Year * 12 + record.Month >= fromKey
                && record.Year * 12 + record.Month <= toKey)
            .OrderBy(record => record.Year)
            .ThenBy(record => record.Month)
            .ToListAsync(cancellationToken);
    }

    public void Add(SalaryRecord record) => context.SalaryRecords.Add(record);

    public void Remove(SalaryRecord record) => context.SalaryRecords.Remove(record);
}
