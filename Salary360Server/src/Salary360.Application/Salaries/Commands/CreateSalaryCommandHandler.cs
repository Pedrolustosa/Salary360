using Salary360.Application.Common.Exceptions;
using Salary360.Application.Interfaces.Identity;
using Salary360.Application.Interfaces.Messaging;
using Salary360.Application.Interfaces.Persistence;
using Salary360.Domain.Entities;
using Salary360.Domain.Interfaces;
using Salary360.Application.Salaries.Dtos;

namespace Salary360.Application.Salaries.Commands;

public sealed class CreateSalaryCommandHandler(
    ISalaryRecordRepository repository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser) : ICommandHandler<CreateSalaryCommand, SalaryRecordDto>
{
    public async Task<SalaryRecordDto> HandleAsync(
        CreateSalaryCommand request, CancellationToken cancellationToken = default)
    {
        var userId = currentUser.GetUserId();
        if (await repository.ExistsAsync(userId, request.Year, request.Month, cancellationToken: cancellationToken))
            throw new ConflictException($"A salary record already exists for {request.Year}-{request.Month:D2}.");

        var record = new SalaryRecord(
            userId,
            request.Year,
            request.Month,
            request.GrossAmount,
            request.Notes,
            request.Items.Select(item => (item.Name.Trim(), item.Type, item.Amount)));

        repository.Add(record);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return record.ToDto();
}
}