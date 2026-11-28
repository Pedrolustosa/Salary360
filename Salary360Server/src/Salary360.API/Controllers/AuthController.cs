using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Salary360.API.Contracts.Requests.Auth;
using Salary360.API.Extensions;
using Salary360.API.Interfaces;
using Salary360.Application.Auth;
using Salary360.Application.Auth.Commands;
using Salary360.Application.Auth.Queries;
using Salary360.Application.Common;
using Salary360.Application.Interfaces.Messaging;
using Salary360.API.Contracts.Responses.Auth;

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
    [ProducesResponseType<UserProfileResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<UserProfileResponse>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var profile = await dispatcher.SendAsync(
            new RegisterCommand(
                request.Email,
                request.Password,
                request.DisplayName,
                request.DateOfBirth,
                request.PhoneNumber,
                request.AcceptedTerms,
                request.AcceptedPrivacyPolicy),
            cancellationToken);
        return StatusCode(StatusCodes.Status201Created, profile.ToResponse());
    }

    [HttpPost("confirm-email")]
    [EnableRateLimiting(ApiServiceCollectionExtensions.RegisterRateLimitPolicy)]
    [AllowAnonymous]
    [EndpointSummary("Confirm e-mail address")]
    [EndpointDescription("Confirms the account using the user id and token from the confirmation link.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ConfirmEmail(ConfirmEmailRequest request, CancellationToken cancellationToken)
    {
        await dispatcher.SendAsync(new ConfirmEmailCommand(request.UserId, request.Token), cancellationToken);
        return NoContent();
    }

    [HttpPost("resend-confirmation")]
    [EnableRateLimiting(ApiServiceCollectionExtensions.RegisterRateLimitPolicy)]
    [AllowAnonymous]
    [EndpointSummary("Resend confirmation e-mail")]
    [EndpointDescription("Requests a new confirmation link. Always returns 202 to avoid revealing whether the e-mail exists.")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> ResendConfirmation(ResendConfirmationRequest request, CancellationToken cancellationToken)
    {
        await dispatcher.SendAsync(new ResendConfirmationCommand(request.Email), cancellationToken);
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
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email ?? string.Empty;
        var password = request.Password ?? string.Empty;
        loginLimiter.Acquire(HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown", email);
        var result = await dispatcher.SendAsync(new LoginCommand(email, password), cancellationToken);
        return Ok(result.ToResponse());
    }

    [HttpGet("me")]
    [Authorize(Policy = AppPermissions.Profile.View)]
    [EndpointSummary("Get current profile")]
    [EndpointDescription("Returns the authenticated user's profile.")]
    [ProducesResponseType<UserProfileResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<UserProfileResponse>> Me(CancellationToken cancellationToken)
        => Ok((await dispatcher.SendAsync(new GetCurrentUserQuery(), cancellationToken)).ToResponse());

    [HttpPut("me")]
    [Authorize(Policy = AppPermissions.Profile.Update)]
    [EndpointSummary("Update current profile")]
    [EndpointDescription("Updates the authenticated user's profile fields.")]
    [ProducesResponseType<UserProfileResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<UserProfileResponse>> UpdateMe(UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var profile = await dispatcher.SendAsync(
            new UpdateProfileCommand(
                request.DisplayName,
                request.DateOfBirth,
                request.PhoneNumber,
                request.ProfilePictureUrl,
                request.Address),
            cancellationToken);
        return Ok(profile.ToResponse());
    }
}