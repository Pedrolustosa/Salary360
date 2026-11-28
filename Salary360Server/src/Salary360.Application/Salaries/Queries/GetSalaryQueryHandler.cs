using Salary360.Application.Common.Exceptions;
using Salary360.Application.Interfaces.Identity;
using Salary360.Application.Interfaces.Messaging;
using Salary360.Application.Interfaces.Persistence;
using Salary360.Application.Salaries.Dtos;

namespace Salary360.Application.Salaries.Queries;

public sealed class GetSalaryQueryHandler(
    ISalaryRecordRepository repository,
    ICurrentUser currentUser) : IQueryHandler<GetSalaryQuery, SalaryRecordDto>
{
    public async Task<SalaryRecordDto> HandleAsync(
        GetSalaryQuery request, CancellationToken cancellationToken = default)
    {
        var record = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (record is null || record.UserId != currentUser.GetUserId())
            throw new NotFoundException("SalaryRecord", request.Id);

        return record.ToDto();
}
}