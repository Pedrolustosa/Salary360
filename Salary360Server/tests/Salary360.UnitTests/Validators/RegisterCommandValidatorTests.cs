using Salary360.Application.Auth.Commands;

namespace Salary360.UnitTests.Validators;

public sealed class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator validator = new();

    private static RegisterCommand Valid() => new(
        "ana@example.com", "Abcdefg1", "Ana", new DateOnly(1990, 3, 15), "+5581999990001", true, true);

    [Fact]
    public void Validate_ValidCommand_Passes()
        => Assert.True(validator.Validate(Valid()).IsValid);

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void Validate_InvalidEmail_Fails(string email)
        => Assert.False(validator.Validate(Valid() with { Email = email }).IsValid);

    [Fact]
    public void Validate_EmailAboveLimit_Fails()
        => Assert.False(validator.Validate(Valid() with { Email = new string('a', 251) + "@x.com" }).IsValid);

    [Theory]
    [InlineData("short1A")]
    [InlineData("alllowercase1")]
    [InlineData("ALLUPPERCASE1")]
    [InlineData("NoDigitsHere")]
    public void Validate_WeakPassword_Fails(string password)
        => Assert.False(validator.Validate(Valid() with { Password = password }).IsValid);

    [Fact]
    public void Validate_DisplayNameAboveLimit_Fails()
        => Assert.False(validator.Validate(Valid() with { DisplayName = new string('a', 101) }).IsValid);

    [Fact]
    public void Validate_FutureBirthDate_Fails()
        => Assert.False(validator.Validate(Valid() with { DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)) }).IsValid);

    [Fact]
    public void Validate_DefaultBirthDate_Fails()
        => Assert.False(validator.Validate(Valid() with { DateOfBirth = default }).IsValid);

    [Theory]
    [InlineData("81999990001")]
    [InlineData("+abc")]
    [InlineData("+55 (81) 99999-0001")]
    public void Validate_PhoneNotInE164_Fails(string phone)
        => Assert.False(validator.Validate(Valid() with { PhoneNumber = phone }).IsValid);

    [Fact]
    public void Validate_NullPhone_Passes()
        => Assert.True(validator.Validate(Valid() with { PhoneNumber = null }).IsValid);

    [Fact]
    public void Validate_TermsNotAccepted_Fails()
        => Assert.False(validator.Validate(Valid() with { AcceptedTerms = false }).IsValid);

    [Fact]
    public void Validate_PrivacyNotAccepted_Fails()
        => Assert.False(validator.Validate(Valid() with { AcceptedPrivacyPolicy = false }).IsValid);
}
