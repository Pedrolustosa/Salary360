using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Salary360.Application.Common;
using Salary360.Application.Common.Exceptions;
using Salary360.Domain.Common;

namespace Salary360.API.Middleware;

public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private static readonly string[] DuplicateCodes = ["DuplicateEmail", "DuplicateUserName"];

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
            return true;

        var problem = Map(exception);

        if (problem is null)
        {
            logger.LogError(exception, "Unhandled exception on {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);
            problem = ApiProblem.Create(
                StatusCodes.Status500InternalServerError, "An unexpected error occurred.", ErrorCodes.InternalError);
        }
        else
        {
            logger.LogWarning("Request failed on {Method} {Path} with {Status}", httpContext.Request.Method, httpContext.Request.Path, problem.Status);
        }

        httpContext.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;

        if (problem.Extensions.TryGetValue(ApiProblem.RetryAfterSecondsKey, out var seconds) && seconds is int retry)
            httpContext.Response.Headers.RetryAfter = retry.ToString(System.Globalization.CultureInfo.InvariantCulture);

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem
        });
    }

    private static ProblemDetails? Map(Exception exception) => exception switch
    {
        ValidationException validation => WithCode(
            new ValidationProblemDetails(ValidationErrorMapper.ToErrors(validation.Errors))
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "One or more validation errors occurred."
            },
            ErrorCodes.ValidationFailed),
        IdentityOperationException identity => MapIdentity(identity),
        DomainException domain => ApiProblem.Create(
            StatusCodes.Status422UnprocessableEntity, "A business rule was violated.", domain.Code),
        NotFoundException => ApiProblem.Create(
            StatusCodes.Status404NotFound, "The resource was not found.", ErrorCodes.NotFound),
        EmailNotConfirmedException => ApiProblem.Create(
            StatusCodes.Status403Forbidden, "The e-mail address has not been confirmed.", ErrorCodes.EmailNotConfirmed),
        UserInactiveException => ApiProblem.Create(
            StatusCodes.Status403Forbidden, "The account is inactive.", ErrorCodes.UserInactive),
        AccountLockedException locked => ApiProblem.WithRetryAfter(
            ApiProblem.Create(StatusCodes.Status403Forbidden, "The account is temporarily locked.", ErrorCodes.AccountLocked),
            locked.RetryAfter),
        TooManyRequestsException tooMany => ApiProblem.WithRetryAfter(
            ApiProblem.Create(StatusCodes.Status429TooManyRequests, "Too many requests.", ErrorCodes.TooManyRequests),
            tooMany.RetryAfter),
        AuthenticationFailedException => ApiProblem.Create(
            StatusCodes.Status401Unauthorized, "Authentication failed.", ErrorCodes.InvalidCredentials),
        _ => null
    };

    private static ProblemDetails MapIdentity(IdentityOperationException identity)
    {
        var duplicate = identity.Codes.Any(code => DuplicateCodes.Contains(code));
        var problem = new ValidationProblemDetails(
            new Dictionary<string, string[]> { ["identity"] = identity.Codes.ToArray() })
        {
            Status = duplicate ? StatusCodes.Status409Conflict : StatusCodes.Status400BadRequest,
            Title = duplicate ? "The resource already exists." : "The identity operation failed."
        };
        problem.Extensions["codes"] = identity.Codes.ToArray();
        return WithCode(problem, duplicate ? ErrorCodes.Conflict : ErrorCodes.IdentityFailed);
    }

    private static ProblemDetails WithCode(ProblemDetails problem, string code)
    {
        problem.Extensions["code"] = code;
        return problem;
    }
}
