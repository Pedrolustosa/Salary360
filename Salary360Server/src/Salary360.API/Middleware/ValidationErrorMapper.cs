using FluentValidation.Results;

namespace Salary360.API.Middleware;

public static class ValidationErrorMapper
{
    public static Dictionary<string, string[]> ToErrors(IEnumerable<ValidationFailure> failures)
        => failures
            .GroupBy(failure => ToFieldName(failure.PropertyName))
            .ToDictionary(
                group => group.Key,
                group => group.Select(failure => ToCode(failure.ErrorCode)).Distinct().ToArray());

    public static string ToFieldName(string propertyName)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
            return "request";

        return string.Join('.', propertyName.Split('.').Select(segment =>
            segment.Length == 0 ? segment : char.ToLowerInvariant(segment[0]) + segment[1..]));
    }

    public static string ToCode(string errorCode) => errorCode switch
    {
        "NotEmptyValidator" or "NotNullValidator" or "NotEqualValidator" => "required",
        "MaximumLengthValidator" => "max_length",
        "MinimumLengthValidator" => "min_length",
        "LengthValidator" => "invalid_length",
        "EmailValidator" => "invalid_email",
        "RegularExpressionValidator" => "invalid_format",
        "EqualValidator" => "must_be_accepted",
        _ when string.IsNullOrEmpty(errorCode) || errorCode.EndsWith("Validator", StringComparison.Ordinal) => "invalid",
        _ => errorCode
    };
}
