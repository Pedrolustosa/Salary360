using NSubstitute;
using Salary360.Application.Common.Exceptions;
using Salary360.Application.Interfaces.Identity;
using Salary360.Application.Interfaces.Persistence;
using Salary360.Application.Salaries.Queries;
using Salary360.Domain.Entities;
using Salary360.Domain.Enums;

namespace Salary360.UnitTests.Application;

public sealed class GetSalaryQueryHandlerTests
{
    private readonly ISalaryRecordRepository repository = Substitute.For<ISalaryRecordRepository>();
    private readonly ICurrentUser currentUser = Substitute.For<ICurrentUser>();

    [Fact]
    public async Task HandleAsync_OtherUsersRecord_ThrowsNotFound()
    {
        var owner = Guid.NewGuid();
        var current = Guid.NewGuid();
        currentUser.GetUserId().Returns(current);
        var record = new SalaryRecord(owner, 2024, 1, 1000m, null, [("A", SalaryItemType.Benefit, 10m)]);
        repository.GetByIdAsync(record.Id, Arg.Any<CancellationToken>()).Returns(record);
        var handler = new GetSalaryQueryHandler(repository, currentUser);

        await Assert.ThrowsAsync<NotFoundException>(() => handler.HandleAsync(new GetSalaryQuery(record.Id)));
    }

    [Fact]
    public async Task HandleAsync_OwnRecord_ReturnsDto()
    {
        var userId = Guid.NewGuid();
        currentUser.GetUserId().Returns(userId);
        var record = new SalaryRecord(userId, 2024, 1, 1000m, null, [("A", SalaryItemType.Benefit, 10m)]);
        repository.GetByIdAsync(record.Id, Arg.Any<CancellationToken>()).Returns(record);
        var handler = new GetSalaryQueryHandler(repository, currentUser);

        var dto = await handler.HandleAsync(new GetSalaryQuery(record.Id));

        Assert.Equal(record.Id, dto.Id);
        Assert.Equal(1010m, dto.NetAmount);
    }
}