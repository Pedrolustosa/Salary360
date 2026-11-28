using Salary360.Application.Common.Interfaces;
using Salary360.Application.Interfaces.Identity;
using Salary360.Application.Interfaces.Messaging;
using Salary360.Application.Interfaces.Persistence;
using Salary360.Application.SalaryIntelligence.Interfaces;
using Salary360.Application.SalaryIntelligence.Dtos;

namespace Salary360.Application.SalaryIntelligence.Queries;

public sealed class GetPurchasingPowerQueryHandler(
    ISalaryRecordRepository repository,
    ICurrentUser currentUser,
    IPurchasingPowerCalculator purchasingPowerCalculator,
    IInflationProvider inflationProvider)
    : IQueryHandler<GetPurchasingPowerQuery, PurchasingPowerDto>
{
    public async Task<PurchasingPowerDto> HandleAsync(
        GetPurchasingPowerQuery request, CancellationToken cancellationToken = default)
    {
        var loaded = await SalaryIntelligenceQuerySupport.LoadAsync(
            currentUser.GetUserId(), request.From, request.To, repository, cancellationToken);

        if (loaded is null)
        {
            return Empty(0, null, null);
        }

        var (from, to, records) = loaded.Value;
        if (records.Count < SalaryAnalysisPeriodResolver.EvolutionRequiredRecords)
            return Empty(records.Count, from, to);

        var inflation = await inflationProvider.GetIndicesAsync(cancellationToken);
        var result = purchasingPowerCalculator.Calculate(
            records,
            request.Basis,
            inflation.Indices,
            inflation.IsStale);

        return new PurchasingPowerDto(
            true,
            SalaryAnalysisPeriodResolver.EvolutionRequiredRecords,
            records.Count,
            SalaryIntelligenceQuerySupport.ToDto(from),
            SalaryIntelligenceQuerySupport.ToDto(to),
            result.InflationAvailable,
            result.IsStale,
            result.Coverage is null
                ? null
                : new InflationCoverageDto(
                    result.Coverage.FromYear,
                    result.Coverage.FromMonth,
                    result.Coverage.ToYear,
                    result.Coverage.ToMonth,
                    result.Coverage.IsPartial),
            result.NominalChangePercent,
            result.IpcaAccumulatedPercent,
            result.RealGainPercent,
            result.RealGainAbsolute,
            result.Series
                .Select(point => new PurchasingPowerPointDto(
                    point.Year,
                    point.Month,
                    point.Actual,
                    point.InflationAdjusted))
                .ToList());
    }

    private static PurchasingPowerDto Empty(int count, Competence? from, Competence? to)
        => new(
            false,
            SalaryAnalysisPeriodResolver.EvolutionRequiredRecords,
            count,
            from is null ? null : SalaryIntelligenceQuerySupport.ToDto(from.Value),
            to is null ? null : SalaryIntelligenceQuerySupport.ToDto(to.Value),
            false,
            false,
            null,
            null,
            null,
            null,
            null,
            []);
}