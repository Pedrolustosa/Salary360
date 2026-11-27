using Salary360.Application.Common.Exceptions;
using Salary360.Application.Interfaces.Identity;
using Salary360.Application.Interfaces.Messaging;
using Salary360.Application.Interfaces.Persistence;
using Salary360.Domain.Interfaces;

namespace Salary360.Application.Salaries.Commands;

public sealed class UpdateSalaryCommandHandler(
    ISalaryRecordRepository repository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser) : ICommandHandler<UpdateSalaryCommand, SalaryRecordDto>
{
    public async Task<SalaryRecordDto> HandleAsync(
        UpdateSalaryCommand request, CancellationToken cancellationToken = default)
    {
        var record = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (record is null || record.UserId != currentUser.UserId)
            throw new NotFoundException("SalaryRecord", request.Id);

        if (await repository.ExistsAsync(
                currentUser.UserId, request.Year, request.Month, request.Id, cancellationToken))
            throw new ConflictException($"A salary record already exists for {request.Year}-{request.Month:D2}.");

        record.Update(
            request.Year,
            request.Month,
            request.GrossAmount,
            request.Notes,
            request.Items.Select(item => (item.Name.Trim(), item.Type, item.Amount)));

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return record.ToDto();
    }
}
