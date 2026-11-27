using NSubstitute;
using Salary360.Application.Common.Exceptions;
using Salary360.Application.Interfaces.Identity;
using Salary360.Application.Interfaces.Persistence;
using Salary360.Application.Salaries.Commands;
using Salary360.Domain.Entities;
using Salary360.Domain.Enums;
using Salary360.Domain.Interfaces;

namespace Salary360.UnitTests.Application;

public sealed class DeleteSalaryCommandHandlerTests
{
    private readonly ISalaryRecordRepository repository = Substitute.For<ISalaryRecordRepository>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUser currentUser = Substitute.For<ICurrentUser>();

    [Fact]
    public async Task HandleAsync_OtherUsersRecord_ThrowsNotFound()
    {
        currentUser.UserId.Returns(Guid.NewGuid());
        var record = new SalaryRecord(Guid.NewGuid(), 2024, 1, 1000m, null, []);
        repository.GetByIdAsync(record.Id, Arg.Any<CancellationToken>()).Returns(record);
        var handler = new DeleteSalaryCommandHandler(repository, unitOfWork, currentUser);

        await Assert.ThrowsAsync<NotFoundException>(() => handler.HandleAsync(new DeleteSalaryCommand(record.Id)));
        repository.DidNotReceiveWithAnyArgs().Remove(default!);
    }
}
