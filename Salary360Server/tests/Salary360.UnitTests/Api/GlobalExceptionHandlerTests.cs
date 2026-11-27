using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Salary360.API.Middleware;
using Salary360.Application.Common.Exceptions;
using Salary360.Domain.Common;

namespace Salary360.UnitTests.Api;

public sealed class GlobalExceptionHandlerTests
{
    private readonly CapturingProblemDetailsService problems = new();

    private async Task<(DefaultHttpContext Context, ProblemDetails Problem)> HandleAsync(Exception exception)
    {
        var handler = new GlobalExceptionHandler(problems, NullLogger<GlobalExceptionHandler>.Instance);
        var context = new DefaultHttpContext();

        var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        Assert.True(handled);
        Assert.NotNull(problems.Last);
        return (context, problems.Last!);
    }

    [Fact]
    public async Task Validation_Returns400WithFieldCodes()
    {
        var failures = new[] { new ValidationFailure("Email", "x") { ErrorCode = "NotEmptyValidator" } };

        var (context, problem) = await HandleAsync(new ValidationException(failures));

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.Equal("validation_failed", problem.Extensions["code"]);
        var details = Assert.IsType<ValidationProblemDetails>(problem);
        Assert.Equal(["required"], details.Errors["email"]);
    }

    [Fact]
    public async Task DuplicateIdentity_Returns409()
    {
        var (context, problem) = await HandleAsync(new IdentityOperationException(["x"], ["DuplicateEmail"]));

        Assert.Equal(StatusCodes.Status409Conflict, context.Response.StatusCode);
        Assert.Equal("conflict", problem.Extensions["code"]);
    }

    [Fact]
    public async Task OtherIdentityFailure_Returns400()
    {
        var (context, problem) = await HandleAsync(new IdentityOperationException(["x"], ["PasswordTooShort"]));

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.Equal("identity_failed", problem.Extensions["code"]);
    }

    [Fact]
    public async Task Domain_Returns422WithItsCode()
    {
        var (context, problem) = await HandleAsync(new DomainException("x", "cannot_deactivate_self"));

        Assert.Equal(StatusCodes.Status422UnprocessableEntity, context.Response.StatusCode);
        Assert.Equal("cannot_deactivate_self", problem.Extensions["code"]);
    }

    [Fact]
    public async Task NotFound_Returns404()
    {
        var (context, problem) = await HandleAsync(new NotFoundException("User", 1));

        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
        Assert.Equal("not_found", problem.Extensions["code"]);
    }

    [Fact]
    public async Task InvalidCredentials_Returns401()
    {
        var (context, problem) = await HandleAsync(new AuthenticationFailedException());

        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        Assert.Equal("invalid_credentials", problem.Extensions["code"]);
    }

    [Fact]
    public async Task EmailNotConfirmed_Returns403()
    {
        var (context, problem) = await HandleAsync(new EmailNotConfirmedException());

        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
        Assert.Equal("email_not_confirmed", problem.Extensions["code"]);
    }

    [Fact]
    public async Task InactiveUser_Returns403()
    {
        var (context, problem) = await HandleAsync(new UserInactiveException());

        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
        Assert.Equal("user_inactive", problem.Extensions["code"]);
    }

    [Fact]
    public async Task AccountLocked_Returns403WithRetryAfter()
    {
        var (context, problem) = await HandleAsync(new AccountLockedException(TimeSpan.FromSeconds(270)));

        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
        Assert.Equal("account_locked", problem.Extensions["code"]);
        Assert.Equal(270, problem.Extensions["retryAfterSeconds"]);
        Assert.Equal("270", context.Response.Headers.RetryAfter.ToString());
    }

    [Fact]
    public async Task TooManyRequests_Returns429WithRetryAfter()
    {
        var (context, problem) = await HandleAsync(new TooManyRequestsException(TimeSpan.FromSeconds(12.2)));

        Assert.Equal(StatusCodes.Status429TooManyRequests, context.Response.StatusCode);
        Assert.Equal("too_many_requests", problem.Extensions["code"]);
        Assert.Equal(13, problem.Extensions["retryAfterSeconds"]);
        Assert.Equal("13", context.Response.Headers.RetryAfter.ToString());
    }

    [Fact]
    public async Task Unexpected_Returns500WithoutLeakingDetails()
    {
        var (context, problem) = await HandleAsync(new InvalidOperationException("secret connection string"));

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.Equal("internal_error", problem.Extensions["code"]);
        Assert.Null(problem.Detail);
        Assert.DoesNotContain("secret", problem.Title ?? string.Empty);
    }
}
