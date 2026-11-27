using Salary360.Domain.Common;
using Salary360.Domain.Enums;

namespace Salary360.Domain.Entities;

public sealed class Expense : AggregateRoot
{
    public const int MaxNameLength = 100;
    public const int MaxNotesLength = 500;
    public const int MaxPerUserPerMonth = 200;
    public const int MaxFutureMonths = 60;
    public const decimal MaxAmount = 999_999_999.99m;

    private Expense()
    {
    }

    public Expense(
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
        DateTime? paidAtUtc = null)
    {
        UserId = userId;
        CategoryId = categoryId;
        SeriesId = seriesId;
        CreatedAtUtc = DateTime.UtcNow;
        Apply(name, type, amount, year, month, dueDay, isPaid, notes, paidAtUtc);
    }

    public Guid UserId { get; private set; }

    public Guid CategoryId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public ExpenseType Type { get; private set; }

    public decimal Amount { get; private set; }

    public int Year { get; private set; }

    public int Month { get; private set; }

    public int? DueDay { get; private set; }

    public bool IsPaid { get; private set; }

    public DateTime? PaidAtUtc { get; private set; }

    public string? Notes { get; private set; }

    public Guid? SeriesId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public void Update(
        Guid categoryId,
        string name,
        ExpenseType type,
        decimal amount,
        int year,
        int month,
        int? dueDay,
        bool isPaid,
        string? notes,
        DateTime? paidAtUtc = null)
    {
        CategoryId = categoryId;
        Apply(name, type, amount, year, month, dueDay, isPaid, notes, paidAtUtc);
    }

    public void UpdateWithoutCompetence(
        Guid categoryId,
        string name,
        ExpenseType type,
        decimal amount,
        int? dueDay,
        bool isPaid,
        string? notes,
        DateTime? paidAtUtc = null)
    {
        CategoryId = categoryId;
        Apply(name, type, amount, Year, Month, dueDay, isPaid, notes, paidAtUtc);
    }

    public void SetPaid(bool isPaid, DateTime? paidAtUtc = null)
    {
        IsPaid = isPaid;
        PaidAtUtc = isPaid ? paidAtUtc ?? DateTime.UtcNow : null;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void ReassignCategory(Guid categoryId)
    {
        CategoryId = categoryId;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public static int ResolveDueDay(int year, int month, int? dueDay)
    {
        if (dueDay is null)
            return 0;

        var lastDay = DateTime.DaysInMonth(year, month);
        return Math.Min(dueDay.Value, lastDay);
    }

    private void Apply(
        string name,
        ExpenseType type,
        decimal amount,
        int year,
        int month,
        int? dueDay,
        bool isPaid,
        string? notes,
        DateTime? paidAtUtc)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Name is required.", "required");

        var trimmedName = name.Trim();
        if (trimmedName.Length > MaxNameLength)
            throw new DomainException("Name exceeds the maximum length.", "max_length");

        if (!Enum.IsDefined(type))
            throw new DomainException("Type is invalid.", "invalid");

        if (amount <= 0 || amount > MaxAmount)
            throw new DomainException("Amount must be positive and within the allowed range.", "must_be_positive");

        if (year is < 1900 or > 2100)
            throw new DomainException("Year is out of range.", "invalid_year");

        if (month is < 1 or > 12)
            throw new DomainException("Month is out of range.", "invalid_month");

        if (dueDay is < 1 or > 31)
            throw new DomainException("Due day is out of range.", "invalid_due_day");

        var trimmedNotes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        if (trimmedNotes is { Length: > MaxNotesLength })
            throw new DomainException("Notes exceed the maximum length.", "max_length");

        if (!isPaid && paidAtUtc is not null)
            throw new DomainException("PaidAtUtc requires IsPaid to be true.", "invalid");

        Name = trimmedName;
        Type = type;
        Amount = Money.Round(amount);
        Year = year;
        Month = month;
        DueDay = dueDay;
        IsPaid = isPaid;
        PaidAtUtc = isPaid ? paidAtUtc ?? DateTime.UtcNow : null;
        Notes = trimmedNotes;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
