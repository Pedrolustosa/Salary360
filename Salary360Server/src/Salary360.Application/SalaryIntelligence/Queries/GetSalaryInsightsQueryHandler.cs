using Salary360.Application.Common.Interfaces;
using Salary360.Application.Interfaces.Identity;
using Salary360.Application.Interfaces.Messaging;
using Salary360.Application.Interfaces.Persistence;
using Salary360.Application.SalaryIntelligence.Interfaces;
using Salary360.Application.SalaryIntelligence.Dtos;

namespace Salary360.Application.SalaryIntelligence.Queries;

public sealed class GetSalaryInsightsQueryHandler(
    ISalaryRecordRepository repository,
    ICurrentUser currentUser,
    ISalaryInsightGenerator insightGenerator,
    IPurchasingPowerCalculator purchasingPowerCalculator,
    IInflationProvider inflationProvider)
    : IQueryHandler<GetSalaryInsightsQuery, SalaryInsightsDto>
{
    public async Task<SalaryInsightsDto> HandleAsync(
        GetSalaryInsightsQuery request, CancellationToken cancellationToken = default)
    {
        var loaded = await SalaryIntelligenceQuerySupport.LoadAsync(
            currentUser.GetUserId(), request.From, request.To, repository, cancellationToken);

        if (loaded is null)
        {
            return new SalaryInsightsDto(
                false,
                SalaryAnalysisPeriodResolver.EvolutionRequiredRecords,
                0,
                null,
                null,
                []);
}

        var (from, to, records) = loaded.Value;
        if (records.Count < SalaryAnalysisPeriodResolver.EvolutionRequiredRecords)
        {
            return new SalaryInsightsDto(
                false,
                SalaryAnalysisPeriodResolver.EvolutionRequiredRecords,
                records.Count,
                SalaryIntelligenceQuerySupport.ToDto(from),
                SalaryIntelligenceQuerySupport.ToDto(to),
                []);
}

        var inflation = await inflationProvider.GetIndicesAsync(cancellationToken);
        var purchasingPower = inflation.Available
            ? purchasingPowerCalculator.Calculate(records, request.Basis, inflation.Indices, inflation.IsStale)
            : null;

        var insights = insightGenerator.Generate(records, purchasingPower, request.Basis);
        return new SalaryInsightsDto(
            true,
            SalaryAnalysisPeriodResolver.EvolutionRequiredRecords,
            records.Count,
            SalaryIntelligenceQuerySupport.ToDto(from),
            SalaryIntelligenceQuerySupport.ToDto(to),
            insights
                .Select(insight => new SalaryInsightDto(
                    insight.Code,
                    insight.Severity,
                    insight.Params,
                    insight.ReferenceCompetence))
                .ToList());
}
}