using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Salary360.UnitTests.Api;

public sealed class CapturingProblemDetailsService : IProblemDetailsService
{
    public ProblemDetails? Last { get; private set; }

    public ValueTask WriteAsync(ProblemDetailsContext context)
    {
        Last = context.ProblemDetails;
        return ValueTask.CompletedTask;
    }

    public ValueTask<bool> TryWriteAsync(ProblemDetailsContext context)
    {
        Last = context.ProblemDetails;
        return ValueTask.FromResult(true);
    }
}