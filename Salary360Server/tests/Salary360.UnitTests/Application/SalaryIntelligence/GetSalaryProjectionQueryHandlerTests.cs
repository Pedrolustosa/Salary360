using NSubstitute;
using Salary360.Application.Common;
using Salary360.Application.Common.Interfaces;
using Salary360.Application.Interfaces.Identity;
using Salary360.Application.Interfaces.Persistence;
using Salary360.Application.SalaryIntelligence.Enums;
using Salary360.Application.SalaryIntelligence.Interfaces;
using Salary360.Application.SalaryIntelligence.Models;
using Salary360.Application.SalaryIntelligence.Queries;
using Salary360.Domain.Entities;

namespace Salary360.UnitTests.Application.SalaryIntelligence;

public sealed class GetSalaryProjectionQueryHandlerTests
{
    private readonly ISalaryRecordRepository repository = Substitute.For<ISalaryRecordRepository>();
    private readonly ICurrentUser currentUser = Substitute.For<ICurrentUser>();
    private readonly ISalaryProjectionCalculator projectionCalculator = Substitute.For<ISalaryProjectionCalculator>();
    private readonly IInflationProvider inflationProvider = Substitute.For<IInflationProvider>();
    private readonly Guid userId = Guid.NewGuid();

    public GetSalaryProjectionQueryHandlerTests()
    {
        currentUser.GetUserId().Returns(userId);
    }

    [Fact]
    public async Task HandleAsync_EmptyUser_HasEnoughDataFalse()
    {
        repository.GetLatestCompetenceAsync(userId, Arg.Any<CancellationToken>()).Returns(((int, int)?)null);
        var handler = CreateHandler();

        var dto = await handler.HandleAsync(new GetSalaryProjectionQuery(null, null));

        Assert.False(dto.HasEnoughData);
        Assert.Equal(6, dto.RequiredRecords);
        Assert.Equal(0, dto.RecordCount);
        Assert.Empty(dto.Scenarios);
    }

    [Fact]
    public async Task HandleAsync_FewerThanSixRecords_HasEnoughDataFalse()
    {
        var records = Enumerable.Range(1, 5)
            .Select(month => new SalaryRecord(userId, 2024, month, 5000m, null, []))
            .ToList();
        repository.ListByPeriodAsync(
                userId, 2024, 1, 2024, 5, Arg.Any<CancellationToken>())
            .Returns(records);
        var handler = CreateHandler();

        var dto = await handler.HandleAsync(new GetSalaryProjectionQuery("2024-01", "2024-05"));

        Assert.False(dto.HasEnoughData);
        Assert.Equal(5, dto.RecordCount);
        await inflationProvider.DidNotReceiveWithAnyArgs().GetIndicesAsync(default);
    }

    [Fact]
    public async Task HandleAsync_IpcaUnavailable_PassesNullIndices()
    {
        var records = Enumerable.Range(1, 6)
            .Select(month => new SalaryRecord(userId, 2024, month, 5000m + month * 100m, null, []))
            .ToList();
        repository.ListByPeriodAsync(
                userId, 2024, 1, 2024, 6, Arg.Any<CancellationToken>())
            .Returns(records);
        inflationProvider.GetIndicesAsync(Arg.Any<CancellationToken>())
            .Returns(InflationSeriesResult.Unavailable);
        projectionCalculator
            .Calculate(records, SalaryBasis.Net, 6, null)
            .Returns(new ProjectionResult(
                "ols_linear",
                6,
                6,
                "projection_estimate",
                null,
                [
                    new ProjectionScenario("conservative", 5000m, null),
                    new ProjectionScenario("likely", 5200m, null),
                    new ProjectionScenario("optimistic", 5400m, null)
                ],
                []));
        var handler = CreateHandler();

        var dto = await handler.HandleAsync(
            new GetSalaryProjectionQuery("2024-01", "2024-06", SalaryBasis.Net, 6));

        Assert.True(dto.HasEnoughData);
        Assert.Null(dto.InflationCriterion);
        Assert.Equal(3, dto.Scenarios.Count);
        projectionCalculator.Received(1).Calculate(records, SalaryBasis.Net, 6, null);
    }

    private GetSalaryProjectionQueryHandler CreateHandler()
        => new(repository, currentUser, projectionCalculator, inflationProvider);
}