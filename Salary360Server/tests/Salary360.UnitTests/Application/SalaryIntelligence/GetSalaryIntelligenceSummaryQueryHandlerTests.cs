using NSubstitute;
using Salary360.Application.Interfaces.Identity;
using Salary360.Application.Interfaces.Persistence;
using Salary360.Application.SalaryIntelligence.Enums;
using Salary360.Application.SalaryIntelligence.Interfaces;
using Salary360.Application.SalaryIntelligence.Models;
using Salary360.Application.SalaryIntelligence.Queries;
using Salary360.Domain.Entities;
using Salary360.Domain.Enums;

namespace Salary360.UnitTests.Application.SalaryIntelligence;

public sealed class GetSalaryIntelligenceSummaryQueryHandlerTests
{
    private readonly ISalaryRecordRepository repository = Substitute.For<ISalaryRecordRepository>();
    private readonly ICurrentUser currentUser = Substitute.For<ICurrentUser>();
    private readonly ISalaryEvolutionCalculator evolutionCalculator = Substitute.For<ISalaryEvolutionCalculator>();
    private readonly Guid userId = Guid.NewGuid();

    public GetSalaryIntelligenceSummaryQueryHandlerTests()
    {
        currentUser.UserId.Returns(userId);
    }

    [Fact]
    public async Task HandleAsync_EmptyUser_HasEnoughDataFalse()
    {
        repository.GetLatestCompetenceAsync(userId, Arg.Any<CancellationToken>()).Returns(((int, int)?)null);
        var handler = CreateHandler();

        var dto = await handler.HandleAsync(new GetSalaryIntelligenceSummaryQuery(null, null));

        Assert.False(dto.HasEnoughData);
        Assert.Equal(2, dto.RequiredRecords);
        Assert.Equal(0, dto.RecordCount);
        Assert.Null(dto.From);
        Assert.Null(dto.To);
        Assert.Null(dto.GrossChangeAbsolute);
        evolutionCalculator.DidNotReceiveWithAnyArgs().Calculate(default!);
    }

    [Fact]
    public async Task HandleAsync_FewerThanTwoRecords_HasEnoughDataFalse()
    {
        var record = new SalaryRecord(userId, 2024, 6, 5000m, null, []);
        repository.GetLatestCompetenceAsync(userId, Arg.Any<CancellationToken>())
            .Returns((2024, 6));
        repository.ListByPeriodAsync(
                userId, Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<SalaryRecord> { record });
        var handler = CreateHandler();

        var dto = await handler.HandleAsync(new GetSalaryIntelligenceSummaryQuery(null, null));

        Assert.False(dto.HasEnoughData);
        Assert.Equal(1, dto.RecordCount);
        Assert.Equal(5000m, dto.CurrentGross);
        Assert.Equal(5000m, dto.CurrentNet);
        Assert.Null(dto.GrossChangePercent);
        evolutionCalculator.DidNotReceiveWithAnyArgs().Calculate(default!);
    }

    [Fact]
    public async Task HandleAsync_EnoughData_ReturnsEvolutionSummary()
    {
        var records = new List<SalaryRecord>
        {
            new(userId, 2024, 1, 4000m, null, []),
            new(userId, 2024, 2, 4400m, null, [])
        };
        repository.ListByPeriodAsync(
                userId, 2024, 1, 2024, 2, Arg.Any<CancellationToken>())
            .Returns(records);
        evolutionCalculator.Calculate(records).Returns(new EvolutionResult(
            new EvolutionChange(400m, 10m),
            new EvolutionChange(400m, 10m),
            [],
            [],
            [],
            [],
            [],
            0m,
            0m,
            0m,
            0m));
        var handler = CreateHandler();

        var dto = await handler.HandleAsync(
            new GetSalaryIntelligenceSummaryQuery("2024-01", "2024-02", SalaryBasis.Gross));

        Assert.True(dto.HasEnoughData);
        Assert.Equal(2, dto.RecordCount);
        Assert.Equal(4400m, dto.CurrentGross);
        Assert.Equal(400m, dto.GrossChangeAbsolute);
        Assert.Equal(10m, dto.GrossChangePercent);
        Assert.Equal(400m, dto.NetChangeAbsolute);
        Assert.Equal(10m, dto.NetChangePercent);
    }

    private GetSalaryIntelligenceSummaryQueryHandler CreateHandler()
        => new(repository, currentUser, evolutionCalculator);
}
