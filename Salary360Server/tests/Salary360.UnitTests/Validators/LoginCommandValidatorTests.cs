using Salary360.Application.Auth.Commands;

namespace Salary360.UnitTests.Validators;

public sealed class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator validator = new();

    [Fact]
    public void Validate_ValidCredentials_Passes()
        => Assert.True(validator.Validate(new LoginCommand("ana@example.com", "anything")).IsValid);

    [Theory]
    [InlineData("", "secret")]
    [InlineData("ana@example.com", "")]
    [InlineData("invalid", "secret")]
    public void Validate_MissingOrMalformedFields_Fails(string email, string password)
        => Assert.False(validator.Validate(new LoginCommand(email, password)).IsValid);
}