using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salary360.API.Contracts.Requests.Expenses;
using Salary360.Application.Common;
using Salary360.Application.Expenses;
using Salary360.Application.Expenses.Categories.Commands;
using Salary360.Application.Expenses.Categories.Queries;
using Salary360.Application.Interfaces.Messaging;
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
    [ProducesResponseType<IReadOnlyList<ExpenseCategoryListItemDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ExpenseCategoryListItemDto>>> List(CancellationToken cancellationToken)
        => Ok(await dispatcher.SendAsync(new ListExpenseCategoriesQuery(), cancellationToken));

    [HttpPost]
    [Authorize(Policy = AppPermissions.Expenses.Create)]
    [EndpointSummary("Create expense category")]
    [ProducesResponseType<ExpenseCategoryDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ExpenseCategoryDto>> Create(
        [FromBody] CreateExpenseCategoryCommand command, CancellationToken cancellationToken)
    {
        var created = await dispatcher.SendAsync(command, cancellationToken);
        return CreatedAtAction(nameof(List), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = AppPermissions.Expenses.Update)]
    [EndpointSummary("Update expense category")]
    [ProducesResponseType<ExpenseCategoryDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ExpenseCategoryDto>> Update(
        Guid id,
        [FromBody] UpdateExpenseCategoryRequest request,
        CancellationToken cancellationToken)
        => Ok(await dispatcher.SendAsync(
            new UpdateExpenseCategoryCommand(id, request.Name, request.ColorKey, request.IconKey),
            cancellationToken));

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
    [ProducesResponseType<CreateDefaultsResultDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<CreateDefaultsResultDto>> CreateDefaults(CancellationToken cancellationToken)
        => Ok(await dispatcher.SendAsync(new CreateDefaultExpenseCategoriesCommand(), cancellationToken));
}
