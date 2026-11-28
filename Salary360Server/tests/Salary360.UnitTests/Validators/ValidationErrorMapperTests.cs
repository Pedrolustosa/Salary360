using FluentValidation.Results;
using Salary360.API.Middleware;

namespace Salary360.UnitTests.Validators;

public sealed class ValidationErrorMapperTests
{
    [Theory]
    [InlineData("Email", "email")]
    [InlineData("Address.PostalCode", "address.postalCode")]
    [InlineData("Permissions[0]", "permissions[0]")]
    [InlineData("", "request")]
    public void ToFieldName_ConvertsToCamelCasePath(string property, string expected)
        => Assert.Equal(expected, ValidationErrorMapper.ToFieldName(property));

    [Theory]
    [InlineData("NotEmptyValidator", "required")]
    [InlineData("MaximumLengthValidator", "max_length")]
    [InlineData("MinimumLengthValidator", "min_length")]
    [InlineData("EmailValidator", "invalid_email")]
    [InlineData("RegularExpressionValidator", "invalid_format")]
    [InlineData("EqualValidator", "must_be_accepted")]
    [InlineData("PredicateValidator", "invalid")]
    [InlineData("password_digit", "password_digit")]
    public void ToCode_MapsFluentValidationCodesToStableCodes(string errorCode, string expected)
        => Assert.Equal(expected, ValidationErrorMapper.ToCode(errorCode));

    [Fact]
    public void ToErrors_GroupsCodesByField()
    {
        var failures = new[]
        {
            new ValidationFailure("Email", "x") { ErrorCode = "NotEmptyValidator" },
            new ValidationFailure("Email", "x") { ErrorCode = "EmailValidator" },
            new ValidationFailure("Password", "x") { ErrorCode = "MinimumLengthValidator" }
        };

        var errors = ValidationErrorMapper.ToErrors(failures);

        Assert.Equal(["required", "invalid_email"], errors["email"]);
        Assert.Equal(["min_length"], errors["password"]);
    }
}