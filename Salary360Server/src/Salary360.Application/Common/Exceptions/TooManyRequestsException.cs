namespace Salary360.Application.Common.Exceptions;

public sealed class TooManyRequestsException(TimeSpan retryAfter) : Exception("Too many requests.")
{
    public TimeSpan RetryAfter { get; } = retryAfter;
}
