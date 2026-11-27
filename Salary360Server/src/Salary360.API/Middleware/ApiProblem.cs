using Microsoft.AspNetCore.Mvc;

namespace Salary360.API.Middleware;

public static class ApiProblem
{
    public const string RetryAfterSecondsKey = "retryAfterSeconds";

    public static ProblemDetails Create(int status, string title, string code)
    {
        var problem = new ProblemDetails { Status = status, Title = title };
        problem.Extensions["code"] = code;
        return problem;
    }

    public static ProblemDetails WithRetryAfter(ProblemDetails problem, TimeSpan retryAfter)
    {
        problem.Extensions[RetryAfterSecondsKey] = SecondsOf(retryAfter);
        return problem;
    }

    public static int SecondsOf(TimeSpan retryAfter) => Math.Max(1, (int)Math.Ceiling(retryAfter.TotalSeconds));
}
