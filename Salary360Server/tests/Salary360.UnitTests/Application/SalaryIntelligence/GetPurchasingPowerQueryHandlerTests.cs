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

public sealed class GetPurchasingPowerQueryHandlerTests
{
    private readonly ISalaryRecordRepository repository = Substitute.For<ISalaryRecordRepository>();
    private readonly ICurrentUser currentUser = Substitute.For<ICurrentUser>();
    private readonly IPurchasingPowerCalculator purchasingPowerCalculator = Substitute.For<IPurchasingPowerCalculator>();
    private readonly IInflationProvider inflationProvider = Substitute.For<IInflationProvider>();
    private readonly Guid userId = Guid.NewGuid();

    public GetPurchasingPowerQueryHandlerTests()
    {
        currentUser.UserId.Returns(userId);
    }

    [Fact]
    public async Task HandleAsync_EmptyUser_HasEnoughDataFalse()
    {
        repository.GetLatestCompetenceAsync(userId, Arg.Any<CancellationToken>()).Returns(((int, int)?)null);
        var handler = CreateHandler();

        var dto = await handler.HandleAsync(new GetPurchasingPowerQuery(null, null));

        Assert.False(dto.HasEnoughData);
        Assert.Equal(0, dto.RecordCount);
        Assert.False(dto.InflationAvailable);
        Assert.Empty(dto.Series);
    }

    [Fact]
    public async Task HandleAsync_FewerThanTwoRecords_HasEnoughDataFalse()
    {
        var record = new SalaryRecord(userId, 2024, 3, 5000m, null, []);
        repository.ListByPeriodAsync(
                userId, 2024, 1, 2024, 3, Arg.Any<CancellationToken>())
            .Returns(new List<SalaryRecord> { record });
        var handler = CreateHandler();

        var dto = await handler.HandleAsync(new GetPurchasingPowerQuery("2024-01", "2024-03"));

        Assert.False(dto.HasEnoughData);
        Assert.Equal(1, dto.RecordCount);
        Assert.False(dto.InflationAvailable);
        await inflationProvider.DidNotReceiveWithAnyArgs().GetIndicesAsync(default);
    }

    [Fact]
    public async Task HandleAsync_IpcaUnavailable_StillReturnsShapeWithInflationAvailableFalse()
    {
        var records = new List<SalaryRecord>
        {
            new(userId, 2024, 1, 5000m, null, []),
            new(userId, 2024, 2, 5200m, null, [])
        };
        repository.ListByPeriodAsync(
                userId, 2024, 1, 2024, 2, Arg.Any<CancellationToken>())
            .Returns(records);
        inflationProvider.GetIndicesAsync(Arg.Any<CancellationToken>())
            .Returns(InflationSeriesResult.Unavailable);
        purchasingPowerCalculator
            .Calculate(records, SalaryBasis.Net, Arg.Any<IReadOnlyDictionary<(int, int), decimal>>(), false)
            .Returns(new PurchasingPowerResult(
                false,
                false,
                null,
                4m,
                null,
                null,
                null,
                [
                    new PurchasingPowerPoint(2024, 1, 5000m, null),
                    new PurchasingPowerPoint(2024, 2, 5200m, null)
                ]));
        var handler = CreateHandler();

        var dto = await handler.HandleAsync(new GetPurchasingPowerQuery("2024-01", "2024-02"));

        Assert.True(dto.HasEnoughData);
        Assert.False(dto.InflationAvailable);
        Assert.Equal(4m, dto.NominalChangePercent);
        Assert.Null(dto.IpcaAccumulatedPercent);
        Assert.Null(dto.RealGainPercent);
        Assert.Equal(2, dto.Series.Count);
    }

    private GetPurchasingPowerQueryHandler CreateHandler()
        => new(repository, currentUser, purchasingPowerCalculator, inflationProvider);
}
