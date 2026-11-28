using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Salary360.IntegrationTests.Support;

namespace Salary360.IntegrationTests;

public sealed class SalaryFlowTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory factory;

    public SalaryFlowTests(ApiFactory factory) => this.factory = factory;

    [Fact]
    public async Task Crud_List_Conflict_Validation_AndIsolation()
    {
        var client = factory.CreateClient();
        var userToken = await client.LoginAsync(ApiFactory.UserEmail, ApiFactory.UserPassword);
        var adminToken = await client.LoginAsync(ApiFactory.AdminEmail, ApiFactory.AdminPassword);
        var user = factory.CreateClient().WithToken(userToken);
        var admin = factory.CreateClient().WithToken(adminToken);

        var now = DateTime.UtcNow;
        var year = now.Year;
        var month = now.Month == 1 ? 1 : now.Month - 1;

        var create = await user.PostAsJsonAsync("/api/salaries", new
        {
            year,
            month,
            grossAmount = 5000m,
            notes = "First",
            items = new[]
            {
                new { name = "VR", type = "Benefit", amount = 400m },
                new { name = "INSS", type = "Deduction", amount = 500m }
            }
        });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var created = await create.ReadJsonAsync();
        var id = created.GetProperty("id").GetGuid();
        Assert.Equal(4900m, created.GetProperty("netAmount").GetDecimal());
        Assert.NotNull(create.Headers.Location);

        var duplicate = await user.PostAsJsonAsync("/api/salaries", new
        {
            year,
            month,
            grossAmount = 1000m,
            items = Array.Empty<object>()
        });
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);

        var invalid = await user.PostAsJsonAsync("/api/salaries", new
        {
            year = 1800,
            month = 13,
            grossAmount = -1m,
            notes = new string('x', 501),
            items = new[] { new { name = "", type = "Benefit", amount = 0m } }
        });
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        var problem = await invalid.ReadJsonAsync();
        Assert.True(problem.TryGetProperty("errors", out _));

        var update = await user.PutAsJsonAsync($"/api/salaries/{id}", new
        {
            year,
            month,
            grossAmount = 6000m,
            notes = "Updated",
            items = new[] { new { name = "Bonus", type = "Benefit", amount = 100m } }
        });
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        var updated = await update.ReadJsonAsync();
        Assert.Equal(6100m, updated.GetProperty("netAmount").GetDecimal());
        Assert.Equal(1, updated.GetProperty("items").GetArrayLength());

        var list = await user.GetAsync($"/api/salaries?year={year}&page=1&pageSize=10");
        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        Assert.True((await list.ReadJsonAsync()).GetProperty("totalCount").GetInt32() >= 1);

        var foreignGet = await admin.GetAsync($"/api/salaries/{id}");
        Assert.Equal(HttpStatusCode.NotFound, foreignGet.StatusCode);

        var foreignDelete = await admin.DeleteAsync($"/api/salaries/{id}");
        Assert.Equal(HttpStatusCode.NotFound, foreignDelete.StatusCode);

        var monthEvolution = await user.GetAsync(
            $"/api/salaries/evolution?mode=Month&month={month}&fromYear={year - 1}&toYear={year}");
        Assert.Equal(HttpStatusCode.OK, monthEvolution.StatusCode);
        var monthPoints = await monthEvolution.ReadJsonAsync();
        Assert.Equal(2, monthPoints.GetArrayLength());
        Assert.Equal(JsonValueKind.Null, monthPoints[0].GetProperty("net").ValueKind);
        Assert.Equal(6100m, monthPoints[1].GetProperty("net").GetDecimal());

        var yearEvolution = await user.GetAsync(
            $"/api/salaries/evolution?mode=Year&fromYear={year - 1}&toYear={year}");
        Assert.Equal(HttpStatusCode.OK, yearEvolution.StatusCode);
        var yearPoints = await yearEvolution.ReadJsonAsync();
        Assert.Equal(JsonValueKind.Null, yearPoints[0].GetProperty("net").ValueKind);
        Assert.Equal(6100m, yearPoints[1].GetProperty("net").GetDecimal());

        var adminEvolution = await admin.GetAsync(
            $"/api/salaries/evolution?mode=Year&fromYear={year - 1}&toYear={year}");
        var adminPoints = await adminEvolution.ReadJsonAsync();
        Assert.Equal(JsonValueKind.Null, adminPoints[1].GetProperty("net").ValueKind);

        var badRange = await user.GetAsync("/api/salaries/evolution?mode=Year&fromYear=2000&toYear=1900");
        Assert.Equal(HttpStatusCode.BadRequest, badRange.StatusCode);

        var delete = await user.DeleteAsync($"/api/salaries/{id}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await user.GetAsync($"/api/salaries/{id}")).StatusCode);
    }

    [Fact]
    public async Task UnauthorizedAndForbidden()
    {
        var anonymous = factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync("/api/salaries")).StatusCode);

        var adminToken = await factory.CreateClient().LoginAsync(ApiFactory.AdminEmail, ApiFactory.AdminPassword);
        var admin = factory.CreateClient().WithToken(adminToken);

        var strip = await admin.PutAsJsonAsync("/api/roles/User/permissions", new
        {
            permissions = new[] { "Profile.View", "Profile.Update" }
        });
        Assert.Equal(HttpStatusCode.OK, strip.StatusCode);

        var userToken = await factory.CreateClient().LoginAsync(ApiFactory.UserEmail, ApiFactory.UserPassword);
        var forbidden = await factory.CreateClient().WithToken(userToken).GetAsync("/api/salaries");
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);

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
}