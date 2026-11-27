namespace Salary360.Application.SalaryIntelligence;

public readonly record struct Competence(int Year, int Month) : IComparable<Competence>
{
    public int Key => Year * 12 + Month;

    public static Competence? TryParse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length != 7 || value[4] != '-')
            return null;

        if (!int.TryParse(value.AsSpan(0, 4), out var year) || !int.TryParse(value.AsSpan(5, 2), out var month))
            return null;

        if (year is < 1900 or > 2100 || month is < 1 or > 12)
            return null;

        return new Competence(year, month);
    }

    public override string ToString() => $"{Year:D4}-{Month:D2}";

    public Competence AddMonths(int months)
    {
        var total = Key + months - 1;
        var year = total / 12;
        var month = total % 12 + 1;
        return new Competence(year, month);
    }

    public int CompareTo(Competence other) => Key.CompareTo(other.Key);

    public static int MonthsBetween(Competence from, Competence to) => to.Key - from.Key + 1;
}
