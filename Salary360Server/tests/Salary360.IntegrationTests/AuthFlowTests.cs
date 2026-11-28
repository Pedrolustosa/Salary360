using System.Net;
using System.Net.Http.Json;
using Salary360.IntegrationTests.Support;

namespace Salary360.IntegrationTests;

public sealed class AuthFlowTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Register_ValidPayload_Returns201AndSendsConfirmationEmail()
    {
        var client = factory.CreateClient();
        var email = $"new-{Guid.NewGuid():N}@example.com";

        var response = await client.PostAsJsonAsync("/api/auth/register", ApiClient.ValidRegistration(email));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Contains(factory.Emails.Sent, sent => sent.To == email);
    }

    [Fact]
    public async Task Register_PayloadThatBypassesTheFront_Returns400WithFieldErrors()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "not-an-email",
            password = "weak",
            displayName = new string('a', 101),
            dateOfBirth = "2999-01-01",
            phoneNumber = "12345",
            acceptedTerms = false,
            acceptedPrivacyPolicy = false
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.ReadJsonAsync();
        Assert.Equal("validation_failed", body.GetProperty("code").GetString());
        var errors = body.GetProperty("errors");
        foreach (var field in new[] { "email", "password", "displayName", "dateOfBirth", "phoneNumber", "acceptedTerms", "acceptedPrivacyPolicy" })
            Assert.True(errors.TryGetProperty(field, out _), $"Expected an error for '{field}'.");
    }

    [Fact]
    public async Task Register_MalformedJson_Returns400WithCode()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsync("/api/auth/register", new StringContent("{ nope", System.Text.Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("validation_failed", (await response.ReadJsonAsync()).GetProperty("code").GetString());
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns409()
    {
        var client = factory.CreateClient();
        var email = $"dup-{Guid.NewGuid():N}@example.com";
        (await client.PostAsJsonAsync("/api/auth/register", ApiClient.ValidRegistration(email))).EnsureSuccessStatusCode();

        var response = await client.PostAsJsonAsync("/api/auth/register", ApiClient.ValidRegistration(email));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("conflict", (await response.ReadJsonAsync()).GetProperty("code").GetString());
    }

    [Fact]
    public async Task Login_UnconfirmedEmail_Returns403WithCode()
    {
        var client = factory.CreateClient();
        var email = $"unconfirmed-{Guid.NewGuid():N}@example.com";
        (await client.PostAsJsonAsync("/api/auth/register", ApiClient.ValidRegistration(email))).EnsureSuccessStatusCode();

        var response = await client.PostAsJsonAsync("/api/auth/login", new { email, password = "Abcdefg1" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal("email_not_confirmed", (await response.ReadJsonAsync()).GetProperty("code").GetString());
    }

    [Fact]
    public async Task Login_WrongPasswordAndUnknownUser_ReturnTheSame401()
    {
        var client = factory.CreateClient();

        var wrongPassword = await client.PostAsJsonAsync("/api/auth/login", new { email = ApiFactory.UserEmail, password = "Wrong#Pass1" });
        var unknownUser = await client.PostAsJsonAsync("/api/auth/login", new { email = "ghost@example.com", password = "Wrong#Pass1" });

        Assert.Equal(HttpStatusCode.Unauthorized, wrongPassword.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, unknownUser.StatusCode);
        Assert.Equal("invalid_credentials", (await wrongPassword.ReadJsonAsync()).GetProperty("code").GetString());
        Assert.Equal("invalid_credentials", (await unknownUser.ReadJsonAsync()).GetProperty("code").GetString());
    }

    [Fact]
    public async Task Login_SeededAdmin_ReturnsTokenWithPermissions()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new { email = ApiFactory.AdminEmail, password = ApiFactory.AdminPassword });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.ReadJsonAsync();
        Assert.False(string.IsNullOrWhiteSpace(body.GetProperty("accessToken").GetString()));
        Assert.Contains("Admin", body.GetProperty("roles").EnumerateArray().Select(role => role.GetString()));
    }

    [Fact]
    public async Task ResendConfirmation_UnknownEmail_Returns202()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/resend-confirmation", new { email = "nobody@example.com" });

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
    }

    [Fact]
    public async Task Me_WithoutToken_Returns401ProblemDetails()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("unauthorized", (await response.ReadJsonAsync()).GetProperty("code").GetString());
    }

    [Fact]
    public async Task UpdateProfile_WithAddress_PersistsAndReturnsIt()
    {
        var client = factory.CreateClient();
        var (_, token) = await client.RegisterAndConfirmAsync(factory, $"profile-{Guid.NewGuid():N}@example.com");
        client.WithToken(token);

        var update = await client.PutAsJsonAsync("/api/auth/me", new
        {
            displayName = "Updated Name",
            dateOfBirth = "1991-04-20",
            phoneNumber = "+5581999990004",
            profilePictureUrl = "https://example.com/avatar.png",
            address = new
            {
                postalCode = "53020-000",
                street = "Rua do Amparo",
                number = "100",
                complement = "Sala 1",
                neighborhood = "Carmo",
                city = "Olinda",
                state = "PE",
                country = "br"
            }
        });

        Assert.Equal(HttpStatusCode.OK, update.StatusCode);

        var me = await (await client.GetAsync("/api/auth/me")).ReadJsonAsync();
        Assert.Equal("Updated Name", me.GetProperty("displayName").GetString());
        Assert.Equal("Olinda", me.GetProperty("address").GetProperty("city").GetString());
    }

    [Fact]
    public async Task UpdateProfile_InvalidPayload_Returns400WithFieldErrors()
    {
        var client = factory.CreateClient();
        var (_, token) = await client.RegisterAndConfirmAsync(factory, $"invalid-{Guid.NewGuid():N}@example.com");
        client.WithToken(token);

        var response = await client.PutAsJsonAsync("/api/auth/me", new
        {
            displayName = "",
            dateOfBirth = "1991-04-20",
            profilePictureUrl = "ftp://example.com/a.png",
            address = new { postalCode = "", street = "", number = "12345678901", neighborhood = "", city = "", state = "", country = "BRA" }
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = (await response.ReadJsonAsync()).GetProperty("errors");
        Assert.True(errors.TryGetProperty("displayName", out _));
        Assert.True(errors.TryGetProperty("profilePictureUrl", out _));
        Assert.True(errors.TryGetProperty("address.country", out _));
        Assert.True(errors.TryGetProperty("address.number", out _));
    }

    [Fact]
    public async Task Login_ThreeWrongPasswords_LocksTheAccountEvenForTheCorrectPassword()
    {
        var client = factory.CreateClient();
        var email = $"lock-{Guid.NewGuid():N}@example.com";
        await client.RegisterAndConfirmAsync(factory, email);

        var first = await client.PostAsJsonAsync("/api/auth/login", new { email, password = "Wrong#Pass1" });
        var second = await client.PostAsJsonAsync("/api/auth/login", new { email, password = "Wrong#Pass1" });
        var third = await client.PostAsJsonAsync("/api/auth/login", new { email, password = "Wrong#Pass1" });
        var correct = await client.PostAsJsonAsync("/api/auth/login", new { email, password = "Abcdefg1" });

        Assert.Equal(HttpStatusCode.Unauthorized, first.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, second.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, third.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, correct.StatusCode);
        var body = await correct.ReadJsonAsync();
        Assert.Equal("account_locked", body.GetProperty("code").GetString());
        Assert.True(body.GetProperty("retryAfterSeconds").GetInt32() > 0);
    }
}