using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salary360.Application.Common;
using Salary360.Application.Interfaces.Messaging;
using Salary360.Application.SalaryIntelligence.Dtos;
using Salary360.Application.SalaryIntelligence.Enums;
using Salary360.Application.SalaryIntelligence.Queries;

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
    [ProducesResponseType<SalaryIntelligenceSummaryDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SalaryIntelligenceSummaryDto>> Summary(
        [FromQuery] string? from,
        [FromQuery] string? to,
        [FromQuery] SalaryBasis basis = SalaryBasis.Net,
        CancellationToken cancellationToken = default)
        => Ok(await dispatcher.SendAsync(new GetSalaryIntelligenceSummaryQuery(from, to, basis), cancellationToken));

    [HttpGet("series")]
    [Authorize(Policy = AppPermissions.Salaries.View)]
    [EndpointSummary("Salary intelligence series")]
    [EndpointDescription("Returns month-by-month salary points for the selected period.")]
    [ProducesResponseType<SalarySeriesDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SalarySeriesDto>> Series(
        [FromQuery] string? from,
        [FromQuery] string? to,
        [FromQuery] SalaryBasis basis = SalaryBasis.Net,
        CancellationToken cancellationToken = default)
        => Ok(await dispatcher.SendAsync(new GetSalarySeriesQuery(from, to, basis), cancellationToken));

    [HttpGet("purchasing-power")]
    [Authorize(Policy = AppPermissions.Salaries.View)]
    [EndpointSummary("Purchasing power")]
    [EndpointDescription("Compares nominal salary variation with accumulated IPCA.")]
    [ProducesResponseType<PurchasingPowerDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PurchasingPowerDto>> PurchasingPower(
        [FromQuery] string? from,
        [FromQuery] string? to,
        [FromQuery] SalaryBasis basis = SalaryBasis.Net,
        CancellationToken cancellationToken = default)
        => Ok(await dispatcher.SendAsync(new GetPurchasingPowerQuery(from, to, basis), cancellationToken));

    [HttpGet("insights")]
    [Authorize(Policy = AppPermissions.Salaries.View)]
    [EndpointSummary("Salary insights")]
    [EndpointDescription("Returns deterministic insight codes for the selected period.")]
    [ProducesResponseType<SalaryInsightsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SalaryInsightsDto>> Insights(
        [FromQuery] string? from,
        [FromQuery] string? to,
        [FromQuery] SalaryBasis basis = SalaryBasis.Net,
        CancellationToken cancellationToken = default)
        => Ok(await dispatcher.SendAsync(new GetSalaryInsightsQuery(from, to, basis), cancellationToken));

    [HttpGet("projection")]
    [Authorize(Policy = AppPermissions.Salaries.View)]
    [EndpointSummary("Salary projection")]
    [EndpointDescription("Returns estimated projection scenarios. Requires at least six salary records.")]
    [ProducesResponseType<SalaryProjectionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SalaryProjectionDto>> Projection(
        [FromQuery] string? from,
        [FromQuery] string? to,
        [FromQuery] SalaryBasis basis = SalaryBasis.Net,
        [FromQuery] int horizon = 6,
        CancellationToken cancellationToken = default)
        => Ok(await dispatcher.SendAsync(new GetSalaryProjectionQuery(from, to, basis, horizon), cancellationToken));
}
