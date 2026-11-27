using Salary360.Application.Common;
using Salary360.Application.Common.Exceptions;
using Salary360.Application.Interfaces.Identity;
using Salary360.Application.Interfaces.Messaging;
using Salary360.Application.Interfaces.Persistence;
using Salary360.Domain.Interfaces;

namespace Salary360.Application.Salaries.Commands;

public sealed class DeleteSalaryCommandHandler(
    ISalaryRecordRepository repository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser) : ICommandHandler<DeleteSalaryCommand, Unit>
{
    public async Task<Unit> HandleAsync(DeleteSalaryCommand request, CancellationToken cancellationToken = default)
    {
        var record = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (record is null || record.UserId != currentUser.UserId)
            throw new NotFoundException("SalaryRecord", request.Id);

        repository.Remove(record);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
