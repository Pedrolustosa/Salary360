using System.Text.RegularExpressions;
using FluentValidation;

namespace Salary360.Application.Auth;

public static class ProfileRules
{
    public const string PhonePattern = @"^\+[1-9][0-9]{7,14}$";

    public static IRuleBuilderOptions<T, string> ValidDisplayName<T>(this IRuleBuilder<T, string> rule)
        => rule.NotEmpty().MaximumLength(100);

    public static IRuleBuilderOptions<T, string> ValidEmail<T>(this IRuleBuilder<T, string> rule)
        => rule.NotEmpty().MaximumLength(256).EmailAddress();

    public static IRuleBuilderOptions<T, string> StrongPassword<T>(this IRuleBuilder<T, string> rule)
        => rule
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(128)
            .Matches("[a-z]").WithErrorCode("password_lowercase")
            .Matches("[A-Z]").WithErrorCode("password_uppercase")
            .Matches("[0-9]").WithErrorCode("password_digit");

    public static IRuleBuilderOptions<T, DateOnly> ValidDateOfBirth<T>(this IRuleBuilder<T, DateOnly> rule)
        => rule
            .NotEqual(default(DateOnly))
            .Must(date => date >= new DateOnly(1900, 1, 1))
            .Must(date => date <= DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)));

    public static IRuleBuilderOptions<T, string?> ValidOptionalProfilePictureUrl<T>(this IRuleBuilder<T, string?> rule)
        => rule.Must(url => string.IsNullOrWhiteSpace(url) || IsHttpUrl(url))
            .MaximumLength(2048);

    public static IRuleBuilderOptions<T, string?> ValidOptionalPhone<T>(this IRuleBuilder<T, string?> rule)
        => rule.Must(phone => string.IsNullOrWhiteSpace(phone) || Regex.IsMatch(phone, PhonePattern));

    private static bool IsHttpUrl(string url)
        => Uri.TryCreate(url, UriKind.Absolute, out var uri)
           && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}
