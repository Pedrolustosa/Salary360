using Salary360.Domain.Common;
using Salary360.Domain.Enums;

namespace Salary360.Domain.Entities;

public sealed class ExpenseCategory : AggregateRoot
{
    public const int MaxNameLength = 60;
    public const int MaxPerUser = 100;

    private ExpenseCategory()
    {
    }

    public ExpenseCategory(
        Guid userId,
        string? name,
        string? systemKey,
        CategoryColor colorKey,
        CategoryIcon iconKey)
    {
        UserId = userId;
        SystemKey = NormalizeSystemKey(systemKey);
        CreatedAtUtc = DateTime.UtcNow;
        Apply(name, colorKey, iconKey, requireName: SystemKey is null);
    }

    public Guid UserId { get; private set; }

    public string? Name { get; private set; }

    public string? SystemKey { get; private set; }

    public CategoryColor ColorKey { get; private set; }

    public CategoryIcon IconKey { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public void Update(string? name, CategoryColor colorKey, CategoryIcon iconKey)
        => Apply(name, colorKey, iconKey, requireName: SystemKey is null || !string.IsNullOrWhiteSpace(name));

    private void Apply(string? name, CategoryColor colorKey, CategoryIcon iconKey, bool requireName)
    {
        if (!Enum.IsDefined(colorKey))
            throw new DomainException("Color is invalid.", "invalid_color");

        if (!Enum.IsDefined(iconKey))
            throw new DomainException("Icon is invalid.", "invalid_icon");

        var trimmed = string.IsNullOrWhiteSpace(name) ? null : name.Trim();
        if (requireName && trimmed is null)
            throw new DomainException("Name is required.", "required");

        if (trimmed is { Length: > MaxNameLength })
            throw new DomainException("Name exceeds the maximum length.", "max_length");

        Name = trimmed;
        ColorKey = colorKey;
        IconKey = iconKey;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    private static string? NormalizeSystemKey(string? systemKey)
    {
        if (string.IsNullOrWhiteSpace(systemKey))
            return null;

        return systemKey.Trim().ToLowerInvariant();
    }
}