using Salary360.Application.Interfaces.Identity;
using Salary360.Application.Interfaces.Messaging;
using Salary360.Application.Interfaces.Persistence;

namespace Salary360.Application.Salaries.Queries;

public sealed class ListSalariesQueryHandler(
    ISalaryRecordRepository repository,
    ICurrentUser currentUser) : IQueryHandler<ListSalariesQuery, PagedSalariesDto>
{
    public async Task<PagedSalariesDto> HandleAsync(
        ListSalariesQuery request, CancellationToken cancellationToken = default)
    {
        var (items, total) = await repository.ListAsync(
            currentUser.UserId, request.Year, request.Page, request.PageSize, cancellationToken);

        return new PagedSalariesDto(
            items.Select(item => item.ToDto()).ToList(),
            request.Page,
            request.PageSize,
            total);
    }
}
