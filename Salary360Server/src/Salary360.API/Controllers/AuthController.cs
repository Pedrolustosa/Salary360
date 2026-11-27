using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Salary360.API.Extensions;
using Salary360.API.Interfaces;
using Salary360.Application.Interfaces.Messaging;
using Salary360.Application.Auth;
using Salary360.Application.Auth.Commands;
using Salary360.Application.Auth.Queries;
using Salary360.Application.Common;

namespace Salary360.API.Controllers;

[ApiController]
[Route("api/auth")]
[Tags("Auth")]
public sealed class AuthController(IDispatcher dispatcher, ILoginAttemptLimiter loginLimiter) : ControllerBase
{
    [HttpPost("register")]
    [EnableRateLimiting(ApiServiceCollectionExtensions.RegisterRateLimitPolicy)]
    [AllowAnonymous]
    [EndpointSummary("Register a new user")]
    [EndpointDescription("Creates an account and sends a confirmation e-mail. The user cannot sign in until the e-mail is confirmed.")]
    [ProducesResponseType<UserProfileDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<UserProfileDto>> Register(RegisterCommand command, CancellationToken cancellationToken)
        => StatusCode(StatusCodes.Status201Created, await dispatcher.SendAsync(command, cancellationToken));

    [HttpPost("confirm-email")]
    [EnableRateLimiting(ApiServiceCollectionExtensions.RegisterRateLimitPolicy)]
    [AllowAnonymous]
    [EndpointSummary("Confirm e-mail address")]
    [EndpointDescription("Confirms the account using the user id and token from the confirmation link.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ConfirmEmail(ConfirmEmailCommand command, CancellationToken cancellationToken)
    {
        await dispatcher.SendAsync(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("resend-confirmation")]
    [EnableRateLimiting(ApiServiceCollectionExtensions.RegisterRateLimitPolicy)]
    [AllowAnonymous]
    [EndpointSummary("Resend confirmation e-mail")]
    [EndpointDescription("Requests a new confirmation link. Always returns 202 to avoid revealing whether the e-mail exists.")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> ResendConfirmation(ResendConfirmationCommand command, CancellationToken cancellationToken)
    {
        await dispatcher.SendAsync(command, cancellationToken);
        return Accepted();
    }

    [HttpPost("login")]
    [EnableRateLimiting(ApiServiceCollectionExtensions.LoginRateLimitPolicy)]
    [AllowAnonymous]
    [EndpointSummary("Sign in")]
    [EndpointDescription("Authenticates with e-mail and password and returns a JWT. Requires a confirmed, active account.")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<AuthResponse>> Login(LoginCommand command, CancellationToken cancellationToken)
    {
        loginLimiter.Acquire(HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown", command.Email ?? string.Empty);
        return Ok(await dispatcher.SendAsync(command, cancellationToken));
    }

    [HttpGet("me")]
    [Authorize(Policy = AppPermissions.Profile.View)]
    [EndpointSummary("Get current profile")]
    [EndpointDescription("Returns the authenticated user's profile.")]
    [ProducesResponseType<UserProfileDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<UserProfileDto>> Me(CancellationToken cancellationToken)
        => Ok(await dispatcher.SendAsync(new GetCurrentUserQuery(), cancellationToken));

    [HttpPut("me")]
    [Authorize(Policy = AppPermissions.Profile.Update)]
    [EndpointSummary("Update current profile")]
    [EndpointDescription("Updates the authenticated user's profile fields.")]
    [ProducesResponseType<UserProfileDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<UserProfileDto>> UpdateMe(UpdateProfileCommand command, CancellationToken cancellationToken)
        => Ok(await dispatcher.SendAsync(command, cancellationToken));
}
