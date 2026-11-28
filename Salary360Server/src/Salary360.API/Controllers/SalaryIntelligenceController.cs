using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salary360.Application.Common;
using Salary360.Application.Interfaces.Messaging;
using Salary360.Application.SalaryIntelligence.Enums;
using Salary360.Application.SalaryIntelligence.Queries;
using Salary360.API.Contracts.Responses.SalaryIntelligence;

namespace Salary360.API.Controllers;

[ApiController]
[Route("api/salary-intelligence")]
[Tags("SalaryIntelligence")]
[Authorize]
public sealed class SalaryIntelligenceController(IDispatcher dispatcher) : ControllerBase
{
    [HttpGet("summary")]
    [Authorize(Policy = AppPermissions.Salaries.View)]
    [EndpointSummary("Salary intelligence summary")]
    [EndpointDescription("Returns current totals and period variation for gross and net salary.")]
    [ProducesResponseType<SalaryIntelligenceSummaryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SalaryIntelligenceSummaryResponse>> Summary(
        [FromQuery] string? from,
        [FromQuery] string? to,
        [FromQuery] SalaryBasis basis = SalaryBasis.Net,
        CancellationToken cancellationToken = default)
        => Ok((await dispatcher.SendAsync(new GetSalaryIntelligenceSummaryQuery(from, to, basis), cancellationToken)).ToResponse());

    [HttpGet("series")]
    [Authorize(Policy = AppPermissions.Salaries.View)]
    [EndpointSummary("Salary intelligence series")]
    [EndpointDescription("Returns month-by-month salary points for the selected period.")]
    [ProducesResponseType<SalarySeriesResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SalarySeriesResponse>> Series(
        [FromQuery] string? from,
        [FromQuery] string? to,
        [FromQuery] SalaryBasis basis = SalaryBasis.Net,
        CancellationToken cancellationToken = default)
        => Ok((await dispatcher.SendAsync(new GetSalarySeriesQuery(from, to, basis), cancellationToken)).ToResponse());

    [HttpGet("purchasing-power")]
    [Authorize(Policy = AppPermissions.Salaries.View)]
    [EndpointSummary("Purchasing power")]
    [EndpointDescription("Compares nominal salary variation with accumulated IPCA.")]
    [ProducesResponseType<PurchasingPowerResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PurchasingPowerResponse>> PurchasingPower(
        [FromQuery] string? from,
        [FromQuery] string? to,
        [FromQuery] SalaryBasis basis = SalaryBasis.Net,
        CancellationToken cancellationToken = default)
        => Ok((await dispatcher.SendAsync(new GetPurchasingPowerQuery(from, to, basis), cancellationToken)).ToResponse());

    [HttpGet("insights")]
    [Authorize(Policy = AppPermissions.Salaries.View)]
    [EndpointSummary("Salary insights")]
    [EndpointDescription("Returns deterministic insight codes for the selected period.")]
    [ProducesResponseType<SalaryInsightsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SalaryInsightsResponse>> Insights(
        [FromQuery] string? from,
        [FromQuery] string? to,
        [FromQuery] SalaryBasis basis = SalaryBasis.Net,
        CancellationToken cancellationToken = default)
        => Ok((await dispatcher.SendAsync(new GetSalaryInsightsQuery(from, to, basis), cancellationToken)).ToResponse());

    [HttpGet("projection")]
    [Authorize(Policy = AppPermissions.Salaries.View)]
    [EndpointSummary("Salary projection")]
    [EndpointDescription("Returns estimated projection scenarios. Requires at least six salary records.")]
    [ProducesResponseType<SalaryProjectionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SalaryProjectionResponse>> Projection(
        [FromQuery] string? from,
        [FromQuery] string? to,
        [FromQuery] SalaryBasis basis = SalaryBasis.Net,
        [FromQuery] int horizon = 6,
        CancellationToken cancellationToken = default)
        => Ok((await dispatcher.SendAsync(new GetSalaryProjectionQuery(from, to, basis, horizon), cancellationToken)).ToResponse());
}