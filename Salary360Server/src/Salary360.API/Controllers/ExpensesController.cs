using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salary360.API.Contracts.Requests.Expenses;
using Salary360.Application.Common;
using Salary360.Application.Expenses;
using Salary360.Application.Expenses.Commands;
using Salary360.Application.Expenses.Queries;
using Salary360.Application.Interfaces.Messaging;
using Salary360.Domain.Enums;
using Salary360.API.Contracts.Responses.Expenses;

namespace Salary360.API.Controllers;

[ApiController]
[Route("api/expenses")]
[Tags("Expenses")]
[Authorize]
public sealed class ExpensesController(IDispatcher dispatcher) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = AppPermissions.Expenses.Create)]
    [EndpointSummary("Create expense")]
    [ProducesResponseType<IReadOnlyList<ExpenseResponse>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<IReadOnlyList<ExpenseResponse>>> Create(
        [FromBody] CreateExpenseRequest request, CancellationToken cancellationToken)
    {
        var created = await dispatcher.SendAsync(
            new CreateExpenseCommand(
                request.CategoryId,
                request.Name,
                request.Type,
                request.Amount,
                request.Year,
                request.Month,
                request.DueDay,
                request.IsPaid,
                request.Notes,
                request.Recurrence,
                request.RecurrenceCount),
            cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created[0].Id }, created.ToResponse());
    }

    [HttpGet]
    [Authorize(Policy = AppPermissions.Expenses.View)]
    [EndpointSummary("List expenses")]
    [ProducesResponseType<PagedExpensesResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedExpensesResponse>> List(
        [FromQuery] int year,
        [FromQuery] int month,
        [FromQuery] Guid? categoryId,
        [FromQuery] ExpenseType? type,
        [FromQuery] bool? isPaid,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await dispatcher.SendAsync(
            new ListExpensesQuery(year, month, categoryId, type, isPaid, page, pageSize),
            cancellationToken);
        return Ok(result.ToResponse());
    }

    [HttpGet("summary")]
    [Authorize(Policy = AppPermissions.Expenses.View)]
    [EndpointSummary("Expense month summary")]
    [ProducesResponseType<ExpenseMonthSummaryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ExpenseMonthSummaryResponse>> Summary(
        [FromQuery] int year,
        [FromQuery] int month,
        CancellationToken cancellationToken = default)
        => Ok((await dispatcher.SendAsync(new GetExpenseMonthSummaryQuery(year, month), cancellationToken)).ToResponse());

    [HttpGet("series")]
    [Authorize(Policy = AppPermissions.Expenses.View)]
    [EndpointSummary("Expense series")]
    [ProducesResponseType<IReadOnlyList<ExpenseSeriesPointResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<ExpenseSeriesPointResponse>>> Series(
        [FromQuery] int fromYear,
        [FromQuery] int fromMonth,
        [FromQuery] int toYear,
        [FromQuery] int toMonth,
        CancellationToken cancellationToken = default)
    {
        var result = await dispatcher.SendAsync(
            new GetExpenseSeriesQuery(fromYear, fromMonth, toYear, toMonth),
            cancellationToken);
        return Ok(result.ToResponse());
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = AppPermissions.Expenses.View)]
    [EndpointSummary("Get expense")]
    [ProducesResponseType<ExpenseResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExpenseResponse>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok((await dispatcher.SendAsync(new GetExpenseQuery(id), cancellationToken)).ToResponse());

    [HttpPut("{id:guid}")]
    [Authorize(Policy = AppPermissions.Expenses.Update)]
    [EndpointSummary("Update expense")]
    [ProducesResponseType<ExpenseResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ExpenseResponse>> Update(
        Guid id,
        [FromQuery] ExpenseChangeScope scope,
        [FromBody] UpdateExpenseRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await dispatcher.SendAsync(
            new UpdateExpenseCommand(
                id,
                request.CategoryId,
                request.Name,
                request.Type,
                request.Amount,
                request.Year,
                request.Month,
                request.DueDay,
                request.IsPaid,
                request.Notes,
                scope),
            cancellationToken);
        return Ok(updated.ToResponse());
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = AppPermissions.Expenses.Delete)]
    [EndpointSummary("Delete expense")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid id,
        [FromQuery] ExpenseChangeScope scope = ExpenseChangeScope.This,
        CancellationToken cancellationToken = default)
    {
        await dispatcher.SendAsync(new DeleteExpenseCommand(id, scope), cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:guid}/paid")]
    [Authorize(Policy = AppPermissions.Expenses.Update)]
    [EndpointSummary("Set expense paid")]
    [ProducesResponseType<ExpenseResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExpenseResponse>> SetPaid(
        Guid id,
        [FromBody] SetExpensePaidRequest request,
        CancellationToken cancellationToken)
        => Ok((await dispatcher.SendAsync(new SetExpensePaidCommand(id, request.IsPaid), cancellationToken)).ToResponse());

    [HttpPost("copy")]
    [Authorize(Policy = AppPermissions.Expenses.Create)]
    [EndpointSummary("Copy expenses")]
    [ProducesResponseType<CopyExpensesResultResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CopyExpensesResultResponse>> Copy(
        [FromBody] CopyExpensesRequest request, CancellationToken cancellationToken)
    {
        var result = await dispatcher.SendAsync(
            new CopyExpensesCommand(
                request.FromYear,
                request.FromMonth,
                request.ToYear,
                request.ToMonth,
                request.OnlyFixed),
            cancellationToken);
        return Ok(result.ToResponse());
    }
}