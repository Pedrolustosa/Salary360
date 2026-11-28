using NSubstitute;
using Salary360.Application.Interfaces.Identity;
using Salary360.Application.Interfaces.Persistence;
using Salary360.Application.SalaryIntelligence.Interfaces;
using Salary360.Application.SalaryIntelligence.Models;
using Salary360.Application.SalaryIntelligence.Queries;
using Salary360.Domain.Entities;

namespace Salary360.UnitTests.Application.SalaryIntelligence;

public sealed class GetSalarySeriesQueryHandlerTests
{
    private readonly ISalaryRecordRepository repository = Substitute.For<ISalaryRecordRepository>();
    private readonly ICurrentUser currentUser = Substitute.For<ICurrentUser>();
    private readonly ISalaryEvolutionCalculator evolutionCalculator = Substitute.For<ISalaryEvolutionCalculator>();
    private readonly Guid userId = Guid.NewGuid();

    public GetSalarySeriesQueryHandlerTests()
    {
        currentUser.GetUserId().Returns(userId);
    }

    [Fact]
    public async Task HandleAsync_EmptyUser_HasEnoughDataFalse()
    {
        repository.GetLatestCompetenceAsync(userId, Arg.Any<CancellationToken>()).Returns(((int, int)?)null);
        var handler = CreateHandler();

        var dto = await handler.HandleAsync(new GetSalarySeriesQuery(null, null));

        Assert.False(dto.HasEnoughData);
        Assert.Equal(0, dto.RecordCount);
        Assert.Empty(dto.Points);
    }

    [Fact]
    public async Task HandleAsync_FewerThanTwoRecords_HasEnoughDataFalse()
    {
        var record = new SalaryRecord(userId, 2024, 1, 5000m, null, []);
        repository.ListByPeriodAsync(
                userId, 2024, 1, 2024, 1, Arg.Any<CancellationToken>())
            .Returns(new List<SalaryRecord> { record });
        var handler = CreateHandler();

        var dto = await handler.HandleAsync(new GetSalarySeriesQuery("2024-01", "2024-01"));

        Assert.False(dto.HasEnoughData);
        Assert.Equal(1, dto.RecordCount);
        Assert.Empty(dto.Points);
    }

    [Fact]
    public async Task HandleAsync_EnoughData_MapsSeriesPoints()
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
            [
                new SeriesPointResult(2024, 1, 4000m, 4000m, 0m, 0m, null, null, null, null),
                new SeriesPointResult(2024, 2, 4400m, 4400m, 0m, 0m, 400m, 10m, 400m, 10m)
            ],
            [],
            [],
            [],
            [],
            0m,
            0m,
            0m,
            0m));
        var handler = CreateHandler();

        var dto = await handler.HandleAsync(new GetSalarySeriesQuery("2024-01", "2024-02"));

        Assert.True(dto.HasEnoughData);
        Assert.Equal(2, dto.Points.Count);
        Assert.Equal(400m, dto.Points[1].GrossChangeAbsolute);
        Assert.Equal(10m, dto.Points[1].GrossChangePercent);
    }

    private GetSalarySeriesQueryHandler CreateHandler()
        => new(repository, currentUser, evolutionCalculator);
}