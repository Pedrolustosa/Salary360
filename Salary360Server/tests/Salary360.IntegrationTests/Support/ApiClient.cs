using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Salary360.IntegrationTests.Support;

public static partial class ApiClient
{
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public static object ValidRegistration(string email, string password = "Abcdefg1") => new
    {
        email,
        password,
        displayName = "Test User",
        dateOfBirth = "1990-03-15",
        phoneNumber = "+5581999990003",
        acceptedTerms = true,
        acceptedPrivacyPolicy = true
    };

    public static async Task<JsonElement> ReadJsonAsync(this HttpResponseMessage response)
        => JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement.Clone();

    public static async Task<string> LoginAsync(this HttpClient client, string email, string password)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
        response.EnsureSuccessStatusCode();
        return (await response.ReadJsonAsync()).GetProperty("accessToken").GetString()!;
    }

    public static HttpClient WithToken(this HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public static async Task<(Guid UserId, string Token)> RegisterAndConfirmAsync(
        this HttpClient client, ApiFactory factory, string email, string password = "Abcdefg1")
    {
        var register = await client.PostAsJsonAsync("/api/auth/register", ValidRegistration(email, password));
        register.EnsureSuccessStatusCode();

        var mail = factory.Emails.Sent.Last(sent => sent.To == email);
        var match = ConfirmationLink().Match(mail.HtmlBody);
        Assert.True(match.Success, "The confirmation link was not found in the e-mail.");

        var confirm = await client.PostAsJsonAsync("/api/auth/confirm-email", new
        {
            userId = Guid.Parse(match.Groups["id"].Value),
            token = match.Groups["token"].Value
        });
        confirm.EnsureSuccessStatusCode();

        var userId = (await register.ReadJsonAsync()).GetProperty("userId").GetGuid();
        return (userId, await client.LoginAsync(email, password));
    }

    [GeneratedRegex(@"userId=(?<id>[0-9a-fA-F-]{36})&(?:amp;)?token=(?<token>[A-Za-z0-9_\-]+)")]
    private static partial Regex ConfirmationLink();
}