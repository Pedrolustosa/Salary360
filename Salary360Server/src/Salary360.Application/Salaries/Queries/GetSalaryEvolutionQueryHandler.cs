using Salary360.Application.Interfaces.Identity;
using Salary360.Application.Interfaces.Messaging;
using Salary360.Application.Interfaces.Persistence;
using Salary360.Application.Salaries.Enums;

namespace Salary360.Application.Salaries.Queries;

public sealed class GetSalaryEvolutionQueryHandler(
    ISalaryRecordRepository repository,
    ICurrentUser currentUser) : IQueryHandler<GetSalaryEvolutionQuery, IReadOnlyList<SalaryEvolutionPointDto>>
{
    public Task<IReadOnlyList<SalaryEvolutionPointDto>> HandleAsync(
        GetSalaryEvolutionQuery request, CancellationToken cancellationToken = default)
        => request.Mode switch
        {
            SalaryEvolutionMode.Month => repository.GetMonthEvolutionAsync(
                currentUser.UserId, request.Month!.Value, request.FromYear, request.ToYear, cancellationToken),
            SalaryEvolutionMode.Year => repository.GetYearEvolutionAsync(
                currentUser.UserId, request.FromYear, request.ToYear, cancellationToken),
            _ => throw new InvalidOperationException("Unsupported evolution mode.")
        };
}
