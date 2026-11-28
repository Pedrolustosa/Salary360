using Salary360.Domain.Common;
using Salary360.Domain.Enums;

namespace Salary360.Domain.Entities;

public sealed class SalaryRecord : AggregateRoot
{
    public const int MaxItems = 50;
    public const int MaxNotesLength = 500;
    public const int MaxItemNameLength = 100;

    private readonly List<SalaryItem> _items = [];

    private SalaryRecord()
    {
    }

    public SalaryRecord(
        Guid userId,
        int year,
        int month,
        decimal grossAmount,
        string? notes,
        IEnumerable<(string Name, SalaryItemType Type, decimal Amount)> items)
    {
        UserId = userId;
        CreatedAtUtc = DateTime.UtcNow;
        Apply(year, month, grossAmount, notes, items);
    }

    public Guid UserId { get; private set; }

    public int Year { get; private set; }

    public int Month { get; private set; }

    public decimal GrossAmount { get; private set; }

    public string? Notes { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<SalaryItem> Items => _items.AsReadOnly();

    public decimal TotalBenefits
        => Money.Round(_items.Where(item => item.Type == SalaryItemType.Benefit).Sum(item => item.Amount));

    public decimal TotalDeductions
        => Money.Round(_items.Where(item => item.Type == SalaryItemType.Deduction).Sum(item => item.Amount));

    public decimal NetAmount
        => Money.Round(GrossAmount + TotalBenefits - TotalDeductions);

    public void Update(
        int year,
        int month,
        decimal grossAmount,
        string? notes,
        IEnumerable<(string Name, SalaryItemType Type, decimal Amount)> items)
        => Apply(year, month, grossAmount, notes, items);

    private void Apply(
        int year,
        int month,
        decimal grossAmount,
        string? notes,
        IEnumerable<(string Name, SalaryItemType Type, decimal Amount)> items)
    {
        if (year is < 1900 or > 2100)
            throw new DomainException("Year is out of range.", "invalid_year");

        if (month is < 1 or > 12)
            throw new DomainException("Month is out of range.", "invalid_month");

        if (grossAmount <= 0)
            throw new DomainException("Gross amount must be positive.", "must_be_positive");

        var itemList = items.ToList();
        if (itemList.Count > MaxItems)
            throw new DomainException("A salary cannot have more than 50 items.", "too_many_items");

        Year = year;
        Month = month;
        GrossAmount = Money.Round(grossAmount);
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        if (Notes is { Length: > MaxNotesLength })
            throw new DomainException("Notes exceed the maximum length.", "max_length");

        while (_items.Count > itemList.Count)
            _items.RemoveAt(_items.Count - 1);

        for (var index = 0; index < itemList.Count; index++)
        {
            var (name, type, amount) = itemList[index];
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Item name is required.", "required");

            if (name.Trim().Length > MaxItemNameLength)
                throw new DomainException("Item name exceeds the maximum length.", "max_length");

            if (!Enum.IsDefined(type))
                throw new DomainException("Item type is invalid.", "invalid");

            if (amount <= 0)
                throw new DomainException("Item amount must be positive.", "must_be_positive");

            if (index < _items.Count)
                _items[index].Apply(name, type, amount, index);
            else
                _items.Add(new SalaryItem(Id, name, type, amount, index));
        }

        UpdatedAtUtc = DateTime.UtcNow;
    }
}