using Salary360.Domain.Common;
using Salary360.Domain.Enums;

namespace Salary360.Domain.Entities;

public sealed class SalaryItem : Entity
{
    private SalaryItem()
    {
    }

    internal SalaryItem(Guid salaryRecordId, string name, SalaryItemType type, decimal amount, int order)
    {
        SalaryRecordId = salaryRecordId;
        Apply(name, type, amount, order);
    }

    public Guid SalaryRecordId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public SalaryItemType Type { get; private set; }

    public decimal Amount { get; private set; }

    public int Order { get; private set; }

    internal void Apply(string name, SalaryItemType type, decimal amount, int order)
    {
        Name = name.Trim();
        Type = type;
        Amount = Money.Round(amount);
        Order = order;
    }
}
