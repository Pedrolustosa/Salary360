using Salary360.Application.Common;
using Salary360.Application.Common.Interfaces;

namespace Salary360.IntegrationTests.Support;

public sealed class FakeInflationProvider : IInflationProvider
{
    public bool Available { get; set; } = true;

    public bool IsStale { get; set; }

    public IReadOnlyDictionary<(int Year, int Month), decimal> Indices { get; set; }
        = new Dictionary<(int Year, int Month), decimal>();

    public Task<InflationSeriesResult> GetIndicesAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(Available
            ? new InflationSeriesResult(true, IsStale, Indices)
            : InflationSeriesResult.Unavailable);
}
