namespace Salary360.Application.Common.Exceptions;

public sealed class AccountLockedException(TimeSpan retryAfter) : Exception("The account is temporarily locked.")
{
    public TimeSpan RetryAfter { get; } = retryAfter;
}