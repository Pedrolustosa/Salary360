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

public sealed class GetSalaryInsightsQueryHandlerTests
{
    private readonly ISalaryRecordRepository repository = Substitute.For<ISalaryRecordRepository>();
    private readonly ICurrentUser currentUser = Substitute.For<ICurrentUser>();
    private readonly ISalaryInsightGenerator insightGenerator = Substitute.For<ISalaryInsightGenerator>();
    private readonly IPurchasingPowerCalculator purchasingPowerCalculator = Substitute.For<IPurchasingPowerCalculator>();
    private readonly IInflationProvider inflationProvider = Substitute.For<IInflationProvider>();
    private readonly Guid userId = Guid.NewGuid();

    public GetSalaryInsightsQueryHandlerTests()
    {
        currentUser.UserId.Returns(userId);
    }

    [Fact]
    public async Task HandleAsync_EmptyUser_HasEnoughDataFalse()
    {
        repository.GetLatestCompetenceAsync(userId, Arg.Any<CancellationToken>()).Returns(((int, int)?)null);
        var handler = CreateHandler();

        var dto = await handler.HandleAsync(new GetSalaryInsightsQuery(null, null));

        Assert.False(dto.HasEnoughData);
        Assert.Equal(0, dto.RecordCount);
        Assert.Empty(dto.Insights);
    }

    [Fact]
    public async Task HandleAsync_FewerThanTwoRecords_HasEnoughDataFalse()
    {
        var record = new SalaryRecord(userId, 2024, 2, 5000m, null, []);
        repository.ListByPeriodAsync(
                userId, 2024, 1, 2024, 2, Arg.Any<CancellationToken>())
            .Returns(new List<SalaryRecord> { record });
        var handler = CreateHandler();

        var dto = await handler.HandleAsync(new GetSalaryInsightsQuery("2024-01", "2024-02"));

        Assert.False(dto.HasEnoughData);
        Assert.Equal(1, dto.RecordCount);
        Assert.Empty(dto.Insights);
    }

    [Fact]
    public async Task HandleAsync_IpcaUnavailable_GeneratesWithoutPurchasingPower()
    {
        var records = new List<SalaryRecord>
        {
            new(userId, 2024, 1, 5000m, null, []),
            new(userId, 2024, 2, 5000m, null, [])
        };
        repository.ListByPeriodAsync(
                userId, 2024, 1, 2024, 2, Arg.Any<CancellationToken>())
            .Returns(records);
        inflationProvider.GetIndicesAsync(Arg.Any<CancellationToken>())
            .Returns(InflationSeriesResult.Unavailable);
        insightGenerator.Generate(records, null, SalaryBasis.Net)
            .Returns([
                new InsightResult("stable_period", InsightSeverity.Info, new Dictionary<string, object>(), null)
            ]);
        var handler = CreateHandler();

        var dto = await handler.HandleAsync(new GetSalaryInsightsQuery("2024-01", "2024-02"));

        Assert.True(dto.HasEnoughData);
        Assert.Single(dto.Insights);
        Assert.Equal("stable_period", dto.Insights[0].Code);
        purchasingPowerCalculator.DidNotReceiveWithAnyArgs()
            .Calculate(default!, default, default!, default);
        insightGenerator.Received(1).Generate(records, null, SalaryBasis.Net);
    }

    private GetSalaryInsightsQueryHandler CreateHandler()
        => new(repository, currentUser, insightGenerator, purchasingPowerCalculator, inflationProvider);
}
