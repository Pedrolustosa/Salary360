using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salary360.API.Contracts.Requests.Expenses;
using Salary360.Application.Common;
using Salary360.Application.Expenses;
using Salary360.Application.Expenses.Categories.Commands;
using Salary360.Application.Expenses.Categories.Queries;
using Salary360.Application.Interfaces.Messaging;
using Salary360.API.Contracts.Responses.Expenses;
namespace Salary360.API.Controllers;

[ApiController]
[Route("api/expense-categories")]
[Tags("ExpenseCategories")]
[Authorize]
public sealed class ExpenseCategoriesController(IDispatcher dispatcher) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = AppPermissions.Expenses.View)]
    [EndpointSummary("List expense categories")]
    [ProducesResponseType<IReadOnlyList<ExpenseCategoryListItemResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ExpenseCategoryListItemResponse>>> List(CancellationToken cancellationToken)
        => Ok((await dispatcher.SendAsync(new ListExpenseCategoriesQuery(), cancellationToken)).ToResponse());

    [HttpPost]
    [Authorize(Policy = AppPermissions.Expenses.Create)]
    [EndpointSummary("Create expense category")]
    [ProducesResponseType<ExpenseCategoryResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ExpenseCategoryResponse>> Create(
        [FromBody] CreateExpenseCategoryRequest request, CancellationToken cancellationToken)
    {
        var created = await dispatcher.SendAsync(
            new CreateExpenseCategoryCommand(request.Name, request.ColorKey, request.IconKey),
            cancellationToken);
        return CreatedAtAction(nameof(List), new { id = created.Id }, created.ToResponse());
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = AppPermissions.Expenses.Update)]
    [EndpointSummary("Update expense category")]
    [ProducesResponseType<ExpenseCategoryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ExpenseCategoryResponse>> Update(
        Guid id,
        [FromBody] UpdateExpenseCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await dispatcher.SendAsync(
            new UpdateExpenseCategoryCommand(id, request.Name, request.ColorKey, request.IconKey),
            cancellationToken);
        return Ok(updated.ToResponse());
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = AppPermissions.Expenses.Delete)]
    [EndpointSummary("Delete expense category")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(
        Guid id,
        [FromQuery] Guid? reassignTo,
        CancellationToken cancellationToken)
    {
        await dispatcher.SendAsync(new DeleteExpenseCategoryCommand(id, reassignTo), cancellationToken);
        return NoContent();
    }

    [HttpPost("defaults")]
    [Authorize(Policy = AppPermissions.Expenses.Create)]
    [EndpointSummary("Create default expense categories")]
    [ProducesResponseType<CreateDefaultsResultResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<CreateDefaultsResultResponse>> CreateDefaults(CancellationToken cancellationToken)
        => Ok((await dispatcher.SendAsync(new CreateDefaultExpenseCategoriesCommand(), cancellationToken)).ToResponse());
}