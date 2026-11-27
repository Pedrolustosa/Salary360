namespace Salary360.Application.Common.Exceptions;

public sealed class IdentityOperationException(IReadOnlyCollection<string> errors, IReadOnlyCollection<string>? codes = null)
    : Exception("The identity operation failed.")
{
    public IReadOnlyCollection<string> Errors { get; } = errors;

    public IReadOnlyCollection<string> Codes { get; } = codes ?? [];
}
