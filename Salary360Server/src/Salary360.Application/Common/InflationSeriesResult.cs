namespace Salary360.Application.Common;

public sealed class InflationSeriesResult
{
    public static InflationSeriesResult Unavailable { get; } = new(false, false, new Dictionary<(int Year, int Month), decimal>());

    public InflationSeriesResult(
        bool available,
        bool isStale,
        IReadOnlyDictionary<(int Year, int Month), decimal> indices)
    {
        Available = available;
        IsStale = isStale;
        Indices = indices;
    }

    public bool Available { get; }

    public bool IsStale { get; }

    public IReadOnlyDictionary<(int Year, int Month), decimal> Indices { get; }
}
