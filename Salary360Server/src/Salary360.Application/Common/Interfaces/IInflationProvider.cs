namespace Salary360.Application.Common.Interfaces;

public interface IInflationProvider
{
    Task<InflationSeriesResult> GetIndicesAsync(CancellationToken cancellationToken = default);
}