using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salary360.Application.Common;
using Salary360.Application.Interfaces.Messaging;
using Salary360.Application.Salaries;
using Salary360.Application.Salaries.Commands;
using Salary360.Application.Salaries.Enums;
using Salary360.Application.Salaries.Queries;

namespace Salary360.API.Controllers;

[ApiController]
[Route("api/salaries")]
[Tags("Salaries")]
[Authorize]
public sealed class SalariesController(IDispatcher dispatcher) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = AppPermissions.Salaries.Create)]
    [EndpointSummary("Create salary")]
    [EndpointDescription("Creates a salary record for a competence month. Returns 409 when the competence already exists.")]
    [ProducesResponseType<SalaryRecordDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SalaryRecordDto>> Create(
        [FromBody] CreateSalaryCommand command, CancellationToken cancellationToken)
    {
        var created = await dispatcher.SendAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpGet]
    [Authorize(Policy = AppPermissions.Salaries.View)]
    [EndpointSummary("List salaries")]
    [EndpointDescription("Lists salary records for the current user, optionally filtered by year.")]
    [ProducesResponseType<PagedSalariesDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedSalariesDto>> List(
        [FromQuery] int? year,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
        => Ok(await dispatcher.SendAsync(new ListSalariesQuery(year, page, pageSize), cancellationToken));

    [HttpGet("evolution")]
    [Authorize(Policy = AppPermissions.Salaries.View)]
    [EndpointSummary("Salary evolution")]
    [EndpointDescription("Returns evolution points for charts. Mode month compares the same month across years; mode year aggregates each year.")]
    [ProducesResponseType<IReadOnlyList<SalaryEvolutionPointDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<SalaryEvolutionPointDto>>> Evolution(
        [FromQuery] SalaryEvolutionMode mode,
        [FromQuery] int? month,
        [FromQuery] int fromYear,
        [FromQuery] int toYear,
        CancellationToken cancellationToken = default)
        => Ok(await dispatcher.SendAsync(
            new GetSalaryEvolutionQuery(mode, month, fromYear, toYear), cancellationToken));

    [HttpGet("{id:guid}")]
    [Authorize(Policy = AppPermissions.Salaries.View)]
    [EndpointSummary("Get salary")]
    [EndpointDescription("Returns a salary record owned by the current user.")]
    [ProducesResponseType<SalaryRecordDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SalaryRecordDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await dispatcher.SendAsync(new GetSalaryQuery(id), cancellationToken));

    [HttpPut("{id:guid}")]
    [Authorize(Policy = AppPermissions.Salaries.Update)]
    [EndpointSummary("Update salary")]
    [EndpointDescription("Replaces salary details and items. Returns 409 when the new competence conflicts.")]
    [ProducesResponseType<SalaryRecordDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SalaryRecordDto>> Update(
        Guid id, [FromBody] UpdateSalaryRequest request, CancellationToken cancellationToken)
        => Ok(await dispatcher.SendAsync(
            new UpdateSalaryCommand(id, request.Year, request.Month, request.GrossAmount, request.Notes, request.Items),
            cancellationToken));

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = AppPermissions.Salaries.Delete)]
    [EndpointSummary("Delete salary")]
    [EndpointDescription("Deletes a salary record owned by the current user.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await dispatcher.SendAsync(new DeleteSalaryCommand(id), cancellationToken);
        return NoContent();
    }
}
