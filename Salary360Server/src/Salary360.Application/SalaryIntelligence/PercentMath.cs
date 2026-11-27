using Salary360.Domain.Common;

namespace Salary360.Application.SalaryIntelligence;

public static class PercentMath
{
    public static decimal RoundPercent(decimal value)
        => Math.Round(value, 2, MidpointRounding.ToEven);

    public static decimal? AbsoluteChange(decimal first, decimal last)
        => Money.Round(last - first);

    public static decimal? PercentChange(decimal first, decimal last)
    {
        if (first == 0m)
            return null;

        return RoundPercent((last - first) / first * 100m);
    }

    public static decimal? Ratio(decimal part, decimal whole)
    {
        if (whole == 0m)
            return null;

        return RoundPercent(part / whole * 100m);
    }
}
