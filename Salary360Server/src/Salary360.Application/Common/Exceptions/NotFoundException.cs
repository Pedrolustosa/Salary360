namespace Salary360.Application.Common.Exceptions;

public sealed class NotFoundException(string resource, object key, string code = "not_found")
    : Exception($"{resource} '{key}' was not found.")
{
    public string Code { get; } = code;
}
