namespace Salary360.Application.Salaries;

public static class SalaryValidation
{
    public const decimal MaxGrossAmount = 999_999_999.99m;

    public static bool IsNotFutureCompetence(int year, int month, TimeProvider time)
    {
        if (year is < 1900 or > 2100 || month is < 1 or > 12)
            return true;

        var now = time.GetUtcNow();
        var competence = new DateOnly(year, month, 1);
        var current = new DateOnly(now.Year, now.Month, 1);
        return competence <= current;
    }

    public static bool HasUniqueItemNames(IEnumerable<SalaryItemInputDto> items)
    {
        var names = items
            .Select(item => item.Name?.Trim() ?? string.Empty)
            .Where(name => name.Length > 0)
            .ToList();
        return names.Count == names.Distinct(StringComparer.OrdinalIgnoreCase).Count();
    }
}
