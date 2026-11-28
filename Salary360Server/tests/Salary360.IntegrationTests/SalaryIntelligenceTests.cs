using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Salary360.IntegrationTests.Support;

namespace Salary360.IntegrationTests;

public sealed class SalaryIntelligenceTests : IClassFixture<SalaryIntelligenceApiFactory>
{
    private static readonly Regex ConfirmationLink = new(
        @"userId=(?<id>[0-9a-fA-F-]{36})&(?:amp;)?token=(?<token>[A-Za-z0-9_\-]+)",
        RegexOptions.Compiled);

    private readonly SalaryIntelligenceApiFactory factory;

    public SalaryIntelligenceTests(SalaryIntelligenceApiFactory factory) => this.factory = factory;

    [Fact]
    public async Task Endpoints_ReturnData_IsolateUsers_AndSurviveUnavailableIpca()
    {
        var now = DateTime.UtcNow;
        SeedInflation(now);

        var userA = await AuthenticatedUserAsync($"{Guid.NewGuid():N}@salary360.test");
        var userB = await AuthenticatedUserAsync($"{Guid.NewGuid():N}@salary360.test");

        for (var offset = 5; offset >= 0; offset--)
        {
            var competence = new DateOnly(now.Year, now.Month, 1).AddMonths(-offset);
            await CreateSalaryAsync(userA, competence.Year, competence.Month, 5000m + offset * 100m);
            await CreateSalaryAsync(userB, competence.Year, competence.Month, 9000m);
        }

        var from = new DateOnly(now.Year, now.Month, 1).AddMonths(-5).ToString("yyyy-MM");
        var to = new DateOnly(now.Year, now.Month, 1).ToString("yyyy-MM");

        foreach (var path in new[]
                 {
                     $"/api/salary-intelligence/summary?from={from}&to={to}&basis=Net",
                     $"/api/salary-intelligence/series?from={from}&to={to}",
                     $"/api/salary-intelligence/purchasing-power?from={from}&to={to}",
                     $"/api/salary-intelligence/insights?from={from}&to={to}",
                     $"/api/salary-intelligence/projection?from={from}&to={to}&horizon=6"
                 })
        {
            var response = await userA.GetAsync(path);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.ReadJsonAsync();
            Assert.True(body.GetProperty("hasEnoughData").GetBoolean());
        }

        var summaryA = await (await userA.GetAsync($"/api/salary-intelligence/summary?from={from}&to={to}")).ReadJsonAsync();
        var summaryB = await (await userB.GetAsync($"/api/salary-intelligence/summary?from={from}&to={to}")).ReadJsonAsync();
        Assert.NotEqual(summaryA.GetProperty("currentGross").GetDecimal(), summaryB.GetProperty("currentGross").GetDecimal());

        factory.Inflation.Available = false;
        var purchasing = await userA.GetAsync($"/api/salary-intelligence/purchasing-power?from={from}&to={to}");
        Assert.Equal(HttpStatusCode.OK, purchasing.StatusCode);
        var purchasingBody = await purchasing.ReadJsonAsync();
        Assert.True(purchasingBody.GetProperty("hasEnoughData").GetBoolean());
        Assert.False(purchasingBody.GetProperty("inflationAvailable").GetBoolean());

        var series = await userA.GetAsync($"/api/salary-intelligence/series?from={from}&to={to}");
        Assert.Equal(HttpStatusCode.OK, series.StatusCode);
        Assert.True((await series.ReadJsonAsync()).GetProperty("hasEnoughData").GetBoolean());
        factory.Inflation.Available = true;
    }

    [Fact]
    public async Task Unauthorized_Forbidden_AndInvalidFilter()
    {
        var anonymous = factory.CreateClient();
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            (await anonymous.GetAsync("/api/salary-intelligence/summary")).StatusCode);

        var adminToken = await factory.CreateClient().LoginAsync(
            SalaryIntelligenceApiFactory.AdminEmail, SalaryIntelligenceApiFactory.AdminPassword);
        var admin = factory.CreateClient().WithToken(adminToken);

        var strip = await admin.PutAsJsonAsync("/api/roles/User/permissions", new
        {
            permissions = new[] { "Profile.View", "Profile.Update" }
        });
        Assert.Equal(HttpStatusCode.OK, strip.StatusCode);

        try
        {
            var userToken = await factory.CreateClient().LoginAsync(
                SalaryIntelligenceApiFactory.UserEmail, SalaryIntelligenceApiFactory.UserPassword);
            var forbidden = await factory.CreateClient().WithToken(userToken)
                .GetAsync("/api/salary-intelligence/summary");
            Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);
        }
        finally
        {
            var restore = await admin.PutAsJsonAsync("/api/roles/User/permissions", new
            {
                permissions = new[]
                {
                    "Profile.View", "Profile.Update",
                    "Salaries.View", "Salaries.Create", "Salaries.Update", "Salaries.Delete",
                    "Expenses.View", "Expenses.Create", "Expenses.Update", "Expenses.Delete"
                }
            });
            Assert.Equal(HttpStatusCode.OK, restore.StatusCode);
        }

        var allowed = await AuthenticatedUserAsync($"{Guid.NewGuid():N}@salary360.test");
        var invalid = await allowed.GetAsync("/api/salary-intelligence/summary?from=2025-06&to=2024-01");
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        var problem = await invalid.ReadJsonAsync();
        Assert.Equal("validation_failed", problem.GetProperty("code").GetString());
        Assert.Contains(
            "invalid_period",
            problem.GetProperty("errors").GetProperty("from").EnumerateArray().Select(item => item.GetString()));
    }

    [Fact]
    public async Task InsufficientRecords_ReturnsHasEnoughDataFalse()
    {
        SeedInflation(DateTime.UtcNow);
        var client = await AuthenticatedUserAsync($"{Guid.NewGuid():N}@salary360.test");
        var now = DateTime.UtcNow;
        var month = now.Month == 1 ? 12 : now.Month - 1;
        var year = now.Month == 1 ? now.Year - 1 : now.Year;
        await CreateSalaryAsync(client, year, month, 4000m);

        var summary = await client.GetAsync("/api/salary-intelligence/summary");
        Assert.Equal(HttpStatusCode.OK, summary.StatusCode);
        var body = await summary.ReadJsonAsync();
        Assert.False(body.GetProperty("hasEnoughData").GetBoolean());
        Assert.Equal(2, body.GetProperty("requiredRecords").GetInt32());

        var projection = await client.GetAsync("/api/salary-intelligence/projection?horizon=3");
        Assert.Equal(HttpStatusCode.OK, projection.StatusCode);
        var projectionBody = await projection.ReadJsonAsync();
        Assert.False(projectionBody.GetProperty("hasEnoughData").GetBoolean());
        Assert.Equal(6, projectionBody.GetProperty("requiredRecords").GetInt32());
    }

    private void SeedInflation(DateTime now)
    {
        var indices = new Dictionary<(int Year, int Month), decimal>();
        var cursor = new DateOnly(now.Year, now.Month, 1).AddMonths(-24);
        var end = new DateOnly(now.Year, now.Month, 1);
        var value = 100m;
        while (cursor <= end)
        {
            indices[(cursor.Year, cursor.Month)] = value;
            value += 0.5m;
            cursor = cursor.AddMonths(1);
        }

        factory.Inflation.Available = true;
        factory.Inflation.IsStale = false;
        factory.Inflation.Indices = indices;
    }

    private async Task<HttpClient> AuthenticatedUserAsync(string email)
    {
        var client = factory.CreateClient();
        var register = await client.PostAsJsonAsync("/api/auth/register", ApiClient.ValidRegistration(email));
        register.EnsureSuccessStatusCode();

        var mail = factory.Emails.Sent.Last(sent => sent.To == email);
        var match = ConfirmationLink.Match(mail.HtmlBody);
        Assert.True(match.Success);

        var confirm = await client.PostAsJsonAsync("/api/auth/confirm-email", new
        {
            userId = Guid.Parse(match.Groups["id"].Value),
            token = match.Groups["token"].Value
        });
        confirm.EnsureSuccessStatusCode();

        var token = await client.LoginAsync(email, "Abcdefg1");
        return factory.CreateClient().WithToken(token);
    }

    private static async Task CreateSalaryAsync(HttpClient client, int year, int month, decimal gross)
    {
        var response = await client.PostAsJsonAsync("/api/salaries", new
        {
            year,
            month,
            grossAmount = gross,
            notes = (string?)null,
            items = new[]
            {
                new { name = "VR", type = "Benefit", amount = 200m },
                new { name = "INSS", type = "Deduction", amount = 300m }
            }
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}