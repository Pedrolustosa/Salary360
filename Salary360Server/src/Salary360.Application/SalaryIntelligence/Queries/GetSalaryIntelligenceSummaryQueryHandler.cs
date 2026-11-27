using Salary360.Application.Interfaces.Identity;
using Salary360.Application.Interfaces.Messaging;
using Salary360.Application.Interfaces.Persistence;
using Salary360.Application.SalaryIntelligence.Dtos;
using Salary360.Application.SalaryIntelligence.Interfaces;

namespace Salary360.Application.SalaryIntelligence.Queries;

public sealed class GetSalaryIntelligenceSummaryQueryHandler(
    ISalaryRecordRepository repository,
    ICurrentUser currentUser,
    ISalaryEvolutionCalculator evolutionCalculator)
    : IQueryHandler<GetSalaryIntelligenceSummaryQuery, SalaryIntelligenceSummaryDto>
{
    public async Task<SalaryIntelligenceSummaryDto> HandleAsync(
        GetSalaryIntelligenceSummaryQuery request, CancellationToken cancellationToken = default)
    {
        var loaded = await SalaryIntelligenceQuerySupport.LoadAsync(
            currentUser.UserId, request.From, request.To, repository, cancellationToken);

        if (loaded is null)
        {
            return new SalaryIntelligenceSummaryDto(
                false,
                SalaryAnalysisPeriodResolver.EvolutionRequiredRecords,
                0,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null);
        }

        var (from, to, records) = loaded.Value;
        if (records.Count < SalaryAnalysisPeriodResolver.EvolutionRequiredRecords)
        {
            var latest = records.Count > 0 ? records[^1] : null;
            return new SalaryIntelligenceSummaryDto(
                false,
                SalaryAnalysisPeriodResolver.EvolutionRequiredRecords,
                records.Count,
                SalaryIntelligenceQuerySupport.ToDto(from),
                SalaryIntelligenceQuerySupport.ToDto(to),
                latest?.GrossAmount,
                latest?.NetAmount,
                latest?.TotalBenefits,
                latest?.TotalDeductions,
                null,
                null,
                null,
                null);
        }

        var evolution = evolutionCalculator.Calculate(records);
        var current = records[^1];
        return new SalaryIntelligenceSummaryDto(
            true,
            SalaryAnalysisPeriodResolver.EvolutionRequiredRecords,
            records.Count,
            SalaryIntelligenceQuerySupport.ToDto(from),
            SalaryIntelligenceQuerySupport.ToDto(to),
            current.GrossAmount,
            current.NetAmount,
            current.TotalBenefits,
            current.TotalDeductions,
            evolution.Gross.Absolute,
            evolution.Gross.Percent,
            evolution.Net.Absolute,
            evolution.Net.Percent);
    }
}
