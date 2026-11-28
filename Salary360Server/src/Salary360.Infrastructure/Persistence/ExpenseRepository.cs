using Microsoft.EntityFrameworkCore;
using Salary360.Application.Expenses;
using Salary360.Application.Interfaces.Persistence;
using Salary360.Domain.Common;
using Salary360.Domain.Entities;
using Salary360.Domain.Enums;
using Salary360.Application.Expenses.Dtos;

namespace Salary360.Infrastructure.Persistence;

public sealed class ExpenseRepository(AppDbContext context) : IExpenseRepository
{
    public Task<Expense?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => context.Expenses.FirstOrDefaultAsync(expense => expense.Id == id, cancellationToken);

    public Task<int> CountInMonthAsync(Guid userId, int year, int month, CancellationToken cancellationToken = default)
        => context.Expenses.CountAsync(
            expense => expense.UserId == userId && expense.Year == year && expense.Month == month,
            cancellationToken);

    public async Task<(IReadOnlyList<ExpenseListItemDto> Items, int Total)> ListAsync(
        Guid userId,
        int year,
        int month,
        Guid? categoryId,
        ExpenseType? type,
        bool? isPaid,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query =
            from expense in context.Expenses.AsNoTracking()
            join category in context.ExpenseCategories.AsNoTracking() on expense.CategoryId equals category.Id
            where expense.UserId == userId && expense.Year == year && expense.Month == month
            select new { expense, category };

        if (categoryId.HasValue)
            query = query.Where(row => row.expense.CategoryId == categoryId.Value);

        if (type.HasValue)
            query = query.Where(row => row.expense.Type == type.Value);

        if (isPaid.HasValue)
            query = query.Where(row => row.expense.IsPaid == isPaid.Value);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(row => row.expense.DueDay == null)
            .ThenBy(row => row.expense.DueDay)
            .ThenBy(row => row.expense.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(row => new ExpenseListItemDto(
                row.expense.Id,
                row.expense.CategoryId,
                row.category.Name,
                row.category.SystemKey,
                row.category.ColorKey,
                row.category.IconKey,
                row.expense.Name,
                row.expense.Type,
                row.expense.Amount,
                row.expense.Year,
                row.expense.Month,
                row.expense.DueDay,
                row.expense.IsPaid,
                row.expense.PaidAtUtc,
                row.expense.SeriesId))
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<ExpenseMonthSummaryDto> GetMonthSummaryAsync(
        Guid userId, int year, int month, CancellationToken cancellationToken = default)
    {
        var expenses = await context.Expenses
            .AsNoTracking()
            .Where(expense => expense.UserId == userId && expense.Year == year && expense.Month == month)
            .GroupBy(_ => 1)
            .Select(group => new
            {
                TotalFixed = group.Where(expense => expense.Type == ExpenseType.Fixed).Sum(expense => expense.Amount),
                TotalVariable = group.Where(expense => expense.Type == ExpenseType.Variable).Sum(expense => expense.Amount),
                TotalPaid = group.Where(expense => expense.IsPaid).Sum(expense => expense.Amount),
                TotalPending = group.Where(expense => !expense.IsPaid).Sum(expense => expense.Amount),
                Count = group.Count()
            })
            .FirstOrDefaultAsync(cancellationToken);

        var totalFixed = Money.Round(expenses?.TotalFixed ?? 0m);
        var totalVariable = Money.Round(expenses?.TotalVariable ?? 0m);
        var total = Money.Round(totalFixed + totalVariable);
        var totalPaid = Money.Round(expenses?.TotalPaid ?? 0m);
        var totalPending = Money.Round(expenses?.TotalPending ?? 0m);
        var count = expenses?.Count ?? 0;

        var byCategoryRows = await (
            from expense in context.Expenses.AsNoTracking()
            join category in context.ExpenseCategories.AsNoTracking() on expense.CategoryId equals category.Id
            where expense.UserId == userId && expense.Year == year && expense.Month == month
            group expense by new
            {
                category.Id,
                category.Name,
                category.SystemKey,
                category.ColorKey,
                category.IconKey
            }
            into grouped
            select new
            {
                grouped.Key.Id,
                grouped.Key.Name,
                grouped.Key.SystemKey,
                grouped.Key.ColorKey,
                grouped.Key.IconKey,
                Total = grouped.Sum(expense => expense.Amount)
            })
            .OrderByDescending(row => row.Total)
            .ToListAsync(cancellationToken);

        var byCategory = byCategoryRows
            .Select(row =>
            {
                var categoryTotal = Money.Round(row.Total);
                var percent = total == 0
                    ? 0m
                    : Money.Round(categoryTotal / total * 100m);
                return new ExpenseCategorySummaryDto(
                    row.Id,
                    row.Name,
                    row.SystemKey,
                    row.ColorKey,
                    row.IconKey,
                    categoryTotal,
                    percent);
            })
            .ToList();

        var salary = await context.SalaryRecords
            .AsNoTracking()
            .Include(record => record.Items)
            .FirstOrDefaultAsync(
                record => record.UserId == userId && record.Year == year && record.Month == month,
                cancellationToken);

        if (salary is null)
        {
            return new ExpenseMonthSummaryDto(
                totalFixed,
                totalVariable,
                total,
                totalPaid,
                totalPending,
                count,
                byCategory,
                false,
                null,
                null,
                null);
        }

        var net = salary.NetAmount;
        var committed = net == 0
            ? 0m
            : Money.Round(total / net * 100m);
        var available = Money.Round(net - total);

        return new ExpenseMonthSummaryDto(
            totalFixed,
            totalVariable,
            total,
            totalPaid,
            totalPending,
            count,
            byCategory,
            true,
            net,
            committed,
            available);
    }

    public async Task<IReadOnlyList<ExpenseSeriesPointDto>> GetSeriesAsync(
        Guid userId, int fromYear, int fromMonth, int toYear, int toMonth, CancellationToken cancellationToken = default)
    {
        var fromKey = fromYear * 100 + fromMonth;
        var toKey = toYear * 100 + toMonth;

        var rows = await context.Expenses
            .AsNoTracking()
            .Where(expense =>
                expense.UserId == userId
                && (expense.Year * 100 + expense.Month) >= fromKey
                && (expense.Year * 100 + expense.Month) <= toKey)
            .GroupBy(expense => new { expense.Year, expense.Month })
            .Select(group => new
            {
                group.Key.Year,
                group.Key.Month,
                TotalFixed = group.Where(expense => expense.Type == ExpenseType.Fixed).Sum(expense => expense.Amount),
                TotalVariable = group.Where(expense => expense.Type == ExpenseType.Variable).Sum(expense => expense.Amount)
            })
            .ToListAsync(cancellationToken);

        var map = rows.ToDictionary(row => (row.Year, row.Month));
        var points = new List<ExpenseSeriesPointDto>();
        var cursor = new DateOnly(fromYear, fromMonth, 1);
        var end = new DateOnly(toYear, toMonth, 1);

        while (cursor <= end)
        {
            map.TryGetValue((cursor.Year, cursor.Month), out var row);
            points.Add(new ExpenseSeriesPointDto(
                cursor.Year,
                cursor.Month,
                Money.Round(row?.TotalFixed ?? 0m),
                Money.Round(row?.TotalVariable ?? 0m)));
            cursor = cursor.AddMonths(1);
        }

        return points;
    }

    public Task<IReadOnlyList<Expense>> ListBySeriesAsync(
        Guid userId, Guid seriesId, CancellationToken cancellationToken = default)
        => ListSeriesInternalAsync(userId, seriesId, null, null, cancellationToken);

    public Task<IReadOnlyList<Expense>> ListBySeriesFromAsync(
        Guid userId, Guid seriesId, int year, int month, CancellationToken cancellationToken = default)
        => ListSeriesInternalAsync(userId, seriesId, year, month, cancellationToken);

    public async Task<IReadOnlyList<Expense>> ListForMonthAsync(
        Guid userId, int year, int month, bool onlyFixed, CancellationToken cancellationToken = default)
    {
        var query = context.Expenses
            .Where(expense => expense.UserId == userId && expense.Year == year && expense.Month == month);

        if (onlyFixed)
            query = query.Where(expense => expense.Type == ExpenseType.Fixed);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<HashSet<(string Name, Guid CategoryId)>> ExistingNameCategoryPairsAsync(
        Guid userId, int year, int month, CancellationToken cancellationToken = default)
    {
        var rows = await context.Expenses
            .AsNoTracking()
            .Where(expense => expense.UserId == userId && expense.Year == year && expense.Month == month)
            .Select(expense => new { expense.Name, expense.CategoryId })
            .ToListAsync(cancellationToken);

        return rows
            .Select(row => (row.Name, row.CategoryId))
            .ToHashSet();
    }

    public void Add(Expense expense) => context.Expenses.Add(expense);

    public void AddRange(IEnumerable<Expense> expenses) => context.Expenses.AddRange(expenses);

    public void Remove(Expense expense) => context.Expenses.Remove(expense);

    public void RemoveRange(IEnumerable<Expense> expenses) => context.Expenses.RemoveRange(expenses);

    private async Task<IReadOnlyList<Expense>> ListSeriesInternalAsync(
        Guid userId,
        Guid seriesId,
        int? fromYear,
        int? fromMonth,
        CancellationToken cancellationToken)
    {
        var query = context.Expenses
            .Where(expense => expense.UserId == userId && expense.SeriesId == seriesId);

        if (fromYear.HasValue && fromMonth.HasValue)
        {
            var key = fromYear.Value * 100 + fromMonth.Value;
            query = query.Where(expense => (expense.Year * 100 + expense.Month) >= key);
        }

        return await query
            .OrderBy(expense => expense.Year)
            .ThenBy(expense => expense.Month)
            .ToListAsync(cancellationToken);
}
}