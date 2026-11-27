namespace Salary360.Domain.Common;

public class DomainException(string message, string code = DomainException.DefaultCode) : Exception(message)
{
    public const string DefaultCode = "business_rule_violation";

    public string Code { get; } = code;
}
