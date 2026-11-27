using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salary360.API.Contracts.Requests.Expenses;
using Salary360.Application.Common;
using Salary360.Application.Expenses;
using Salary360.Application.Expenses.Commands;
using Salary360.Application.Expenses.Queries;
using Salary360.Application.Interfaces.Messaging;
using Salary360.Domain.Enums;

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
    [ProducesResponseType<IReadOnlyList<ExpenseDto>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<IReadOnlyList<ExpenseDto>>> Create(
        [FromBody] CreateExpenseCommand command, CancellationToken cancellationToken)
    {
        var created = await dispatcher.SendAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created[0].Id }, created);
    }

    [HttpGet]
    [Authorize(Policy = AppPermissions.Expenses.View)]
    [EndpointSummary("List expenses")]
    [ProducesResponseType<PagedExpensesDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedExpensesDto>> List(
        [FromQuery] int year,
        [FromQuery] int month,
        [FromQuery] Guid? categoryId,
        [FromQuery] ExpenseType? type,
        [FromQuery] bool? isPaid,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
        => Ok(await dispatcher.SendAsync(
            new ListExpensesQuery(year, month, categoryId, type, isPaid, page, pageSize),
            cancellationToken));

    [HttpGet("summary")]
    [Authorize(Policy = AppPermissions.Expenses.View)]
    [EndpointSummary("Expense month summary")]
    [ProducesResponseType<ExpenseMonthSummaryDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ExpenseMonthSummaryDto>> Summary(
        [FromQuery] int year,
        [FromQuery] int month,
        CancellationToken cancellationToken = default)
        => Ok(await dispatcher.SendAsync(new GetExpenseMonthSummaryQuery(year, month), cancellationToken));

    [HttpGet("series")]
    [Authorize(Policy = AppPermissions.Expenses.View)]
    [EndpointSummary("Expense series")]
    [ProducesResponseType<IReadOnlyList<ExpenseSeriesPointDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<ExpenseSeriesPointDto>>> Series(
        [FromQuery] int fromYear,
        [FromQuery] int fromMonth,
        [FromQuery] int toYear,
        [FromQuery] int toMonth,
        CancellationToken cancellationToken = default)
        => Ok(await dispatcher.SendAsync(
            new GetExpenseSeriesQuery(fromYear, fromMonth, toYear, toMonth),
            cancellationToken));

    [HttpGet("{id:guid}")]
    [Authorize(Policy = AppPermissions.Expenses.View)]
    [EndpointSummary("Get expense")]
    [ProducesResponseType<ExpenseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExpenseDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await dispatcher.SendAsync(new GetExpenseQuery(id), cancellationToken));

    [HttpPut("{id:guid}")]
    [Authorize(Policy = AppPermissions.Expenses.Update)]
    [EndpointSummary("Update expense")]
    [ProducesResponseType<ExpenseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ExpenseDto>> Update(
        Guid id,
        [FromQuery] ExpenseChangeScope scope,
        [FromBody] UpdateExpenseRequest request,
        CancellationToken cancellationToken)
        => Ok(await dispatcher.SendAsync(
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
            cancellationToken));

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
    [ProducesResponseType<ExpenseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExpenseDto>> SetPaid(
        Guid id,
        [FromBody] SetExpensePaidRequest request,
        CancellationToken cancellationToken)
        => Ok(await dispatcher.SendAsync(new SetExpensePaidCommand(id, request.IsPaid), cancellationToken));

    [HttpPost("copy")]
    [Authorize(Policy = AppPermissions.Expenses.Create)]
    [EndpointSummary("Copy expenses")]
    [ProducesResponseType<CopyExpensesResultDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CopyExpensesResultDto>> Copy(
        [FromBody] CopyExpensesCommand command, CancellationToken cancellationToken)
        => Ok(await dispatcher.SendAsync(command, cancellationToken));
}
