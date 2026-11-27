using Salary360.Application.Auth;
using Salary360.Application.Auth.Commands;

namespace Salary360.UnitTests.Validators;

public sealed class UpdateProfileCommandValidatorTests
{
    private readonly UpdateProfileCommandValidator validator = new();

    private static UserAddressDto ValidAddress() => new(
        "53020-000", "Rua do Amparo", "100", null, "Carmo", "Olinda", "PE", "BR");

    private static UpdateProfileCommand Valid() => new(
        "Ana", new DateOnly(1990, 3, 15), "+5581999990001", "https://example.com/a.png", ValidAddress());

    [Fact]
    public void Validate_ValidCommand_Passes()
        => Assert.True(validator.Validate(Valid()).IsValid);

    [Fact]
    public void Validate_NoAddress_Passes()
        => Assert.True(validator.Validate(Valid() with { Address = null }).IsValid);

    [Theory]
    [InlineData("ftp://example.com/a.png")]
    [InlineData("not a url")]
    [InlineData("/relative/a.png")]
    public void Validate_InvalidPictureUrl_Fails(string url)
        => Assert.False(validator.Validate(Valid() with { ProfilePictureUrl = url }).IsValid);

    [Fact]
    public void Validate_PictureUrlAboveLimit_Fails()
        => Assert.False(validator.Validate(Valid() with { ProfilePictureUrl = "https://example.com/" + new string('a', 2050) }).IsValid);

    [Fact]
    public void Validate_EmptyDisplayName_Fails()
        => Assert.False(validator.Validate(Valid() with { DisplayName = " " }).IsValid);

    [Fact]
    public void Validate_AddressWithMissingFields_Fails()
        => Assert.False(validator.Validate(Valid() with { Address = ValidAddress() with { Street = "" } }).IsValid);

    [Fact]
    public void Validate_AddressNumberAboveLimit_Fails()
        => Assert.False(validator.Validate(Valid() with { Address = ValidAddress() with { Number = "12345678901" } }).IsValid);

    [Theory]
    [InlineData("BRA")]
    [InlineData("B")]
    [InlineData("12")]
    public void Validate_InvalidCountry_Fails(string country)
        => Assert.False(validator.Validate(Valid() with { Address = ValidAddress() with { Country = country } }).IsValid);
}
