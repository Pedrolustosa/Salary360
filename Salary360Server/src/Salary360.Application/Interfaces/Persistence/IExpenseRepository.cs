using Salary360.Application.Expenses;
using Salary360.Domain.Entities;
using Salary360.Domain.Enums;

namespace Salary360.Application.Interfaces.Persistence;

public interface IExpenseRepository
{
    Task<Expense?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<int> CountInMonthAsync(Guid userId, int year, int month, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<ExpenseListItemDto> Items, int Total)> ListAsync(
        Guid userId,
        int year,
        int month,
        Guid? categoryId,
        ExpenseType? type,
        bool? isPaid,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<ExpenseMonthSummaryDto> GetMonthSummaryAsync(
        Guid userId, int year, int month, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ExpenseSeriesPointDto>> GetSeriesAsync(
        Guid userId, int fromYear, int fromMonth, int toYear, int toMonth, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Expense>> ListBySeriesAsync(
        Guid userId, Guid seriesId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Expense>> ListBySeriesFromAsync(
        Guid userId, Guid seriesId, int year, int month, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Expense>> ListForMonthAsync(
        Guid userId, int year, int month, bool onlyFixed, CancellationToken cancellationToken = default);

    Task<HashSet<(string Name, Guid CategoryId)>> ExistingNameCategoryPairsAsync(
        Guid userId, int year, int month, CancellationToken cancellationToken = default);

    void Add(Expense expense);

    void AddRange(IEnumerable<Expense> expenses);

    void Remove(Expense expense);

    void RemoveRange(IEnumerable<Expense> expenses);
}
