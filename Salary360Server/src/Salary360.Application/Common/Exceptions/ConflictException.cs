namespace Salary360.Application.Common.Exceptions;

public sealed class ConflictException(string message, string code = "conflict")
    : Exception(message)
{
    public string Code { get; } = code;
}
