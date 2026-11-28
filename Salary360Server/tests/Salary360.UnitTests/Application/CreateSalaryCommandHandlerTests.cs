using NSubstitute;
using Salary360.Application.Common.Exceptions;
using Salary360.Application.Interfaces.Identity;
using Salary360.Application.Interfaces.Persistence;
using Salary360.Application.Salaries;
using Salary360.Application.Salaries.Commands;
using Salary360.Application.Salaries.Dtos;
using Salary360.Domain.Entities;
using Salary360.Domain.Enums;
using Salary360.Domain.Interfaces;

namespace Salary360.UnitTests.Application;

public sealed class CreateSalaryCommandHandlerTests
{
    private readonly ISalaryRecordRepository repository = Substitute.For<ISalaryRecordRepository>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUser currentUser = Substitute.For<ICurrentUser>();

    [Fact]
    public async Task HandleAsync_DuplicateCompetence_ThrowsConflict()
    {
        var userId = Guid.NewGuid();
        currentUser.GetUserId().Returns(userId);
        repository.ExistsAsync(userId, 2024, 1, null, Arg.Any<CancellationToken>()).Returns(true);
        var handler = new CreateSalaryCommandHandler(repository, unitOfWork, currentUser);

        await Assert.ThrowsAsync<ConflictException>(() => handler.HandleAsync(Command(2024, 1)));
        repository.DidNotReceiveWithAnyArgs().Add(default!);
    }

    [Fact]
    public async Task HandleAsync_CreatesForCurrentUser()
    {
        var userId = Guid.NewGuid();
        currentUser.GetUserId().Returns(userId);
        repository.ExistsAsync(userId, 2024, 2, null, Arg.Any<CancellationToken>()).Returns(false);
        var handler = new CreateSalaryCommandHandler(repository, unitOfWork, currentUser);

        var dto = await handler.HandleAsync(Command(2024, 2));

        Assert.Equal(2024, dto.Year);
        Assert.Equal(2, dto.Month);
        repository.Received(1).Add(Arg.Is<SalaryRecord>(record => record.UserId == userId));
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static CreateSalaryCommand Command(int year, int month)
        => new(year, month, 4000m, null, [new SalaryItemInputDto("VR", SalaryItemType.Benefit, 200m)]);
}