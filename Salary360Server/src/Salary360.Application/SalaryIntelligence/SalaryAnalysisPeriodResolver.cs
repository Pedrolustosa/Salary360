using Salary360.Application.Interfaces.Persistence;

namespace Salary360.Application.SalaryIntelligence;

public static class SalaryAnalysisPeriodResolver
{
    public const int DefaultWindowMonths = 12;
    public const int MaxWindowMonths = 120;
    public const int EvolutionRequiredRecords = 2;
    public const int ProjectionRequiredRecords = 6;

    public static async Task<(Competence From, Competence To)?> ResolveAsync(
        Guid userId,
        string? from,
        string? to,
        ISalaryRecordRepository repository,
        CancellationToken cancellationToken)
    {
        var parsedFrom = Competence.TryParse(from);
        var parsedTo = Competence.TryParse(to);

        if (parsedFrom is not null && parsedTo is not null)
            return (parsedFrom.Value, parsedTo.Value);

        var latest = await repository.GetLatestCompetenceAsync(userId, cancellationToken);
        if (latest is null)
            return null;

        var end = new Competence(latest.Value.Year, latest.Value.Month);
        var start = end.AddMonths(-(DefaultWindowMonths - 1));
        return (start, end);
    }
}