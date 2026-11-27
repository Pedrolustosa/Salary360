using System.Net;
using System.Net.Http.Json;
using Salary360.IntegrationTests.Support;

namespace Salary360.IntegrationTests;

public sealed class AdministrationTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private async Task<HttpClient> AdminClientAsync()
    {
        var client = factory.CreateClient();
        return client.WithToken(await client.LoginAsync(ApiFactory.AdminEmail, ApiFactory.AdminPassword));
    }

    [Fact]
    public async Task ListUsers_AsRegularUser_Returns403()
    {
        var client = factory.CreateClient();
        client.WithToken(await client.LoginAsync(ApiFactory.UserEmail, ApiFactory.UserPassword));

        var response = await client.GetAsync("/api/users");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal("forbidden", (await response.ReadJsonAsync()).GetProperty("code").GetString());
    }

    [Fact]
    public async Task ListUsers_AsAdmin_ReturnsSeededUsers()
    {
        var client = await AdminClientAsync();

        var response = await client.GetAsync("/api/users");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var emails = (await response.ReadJsonAsync()).EnumerateArray().Select(user => user.GetProperty("email").GetString()).ToList();
        Assert.Contains(ApiFactory.AdminEmail, emails);
        Assert.Contains(ApiFactory.UserEmail, emails);
    }

    [Fact]
    public async Task SetStatus_DeactivatingYourself_Returns422()
    {
        var client = await AdminClientAsync();
        var users = await (await client.GetAsync("/api/users")).ReadJsonAsync();
        var adminId = users.EnumerateArray().First(user => user.GetProperty("email").GetString() == ApiFactory.AdminEmail).GetProperty("userId").GetGuid();

        var response = await client.PutAsJsonAsync($"/api/users/{adminId}/status", new { status = "Inactive" });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal("cannot_deactivate_self", (await response.ReadJsonAsync()).GetProperty("code").GetString());
    }

    [Fact]
    public async Task SetStatus_InvalidStatus_Returns400()
    {
        var client = await AdminClientAsync();

        var response = await client.PutAsJsonAsync($"/api/users/{Guid.NewGuid()}/status", new { status = "Banana" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SetStatus_UnknownUser_Returns404()
    {
        var client = await AdminClientAsync();

        var response = await client.PutAsJsonAsync($"/api/users/{Guid.NewGuid()}/status", new { status = "Inactive" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("not_found", (await response.ReadJsonAsync()).GetProperty("code").GetString());
    }

    [Fact]
    public async Task DeactivatedUser_LosesAccessImmediatelyAndCannotLogIn()
    {
        var admin = await AdminClientAsync();
        var userClient = factory.CreateClient();
        var (userId, token) = await userClient.RegisterAndConfirmAsync(factory, $"victim-{Guid.NewGuid():N}@example.com");
        userClient.WithToken(token);
        Assert.Equal(HttpStatusCode.OK, (await userClient.GetAsync("/api/auth/me")).StatusCode);

        var deactivate = await admin.PutAsJsonAsync($"/api/users/{userId}/status", new { status = "Inactive" });
        Assert.Equal(HttpStatusCode.OK, deactivate.StatusCode);

        Assert.Equal(HttpStatusCode.Unauthorized, (await userClient.GetAsync("/api/auth/me")).StatusCode);
    }

    [Fact]
    public async Task UpdatePermissions_AdminRoleIsLocked()
    {
        var client = await AdminClientAsync();

        var response = await client.PutAsJsonAsync("/api/roles/Admin/permissions", new { permissions = new[] { "Profile.View" } });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("identity_failed", (await response.ReadJsonAsync()).GetProperty("code").GetString());
    }

    [Fact]
    public async Task UpdatePermissions_UnknownPermission_Returns400()
    {
        var client = await AdminClientAsync();

        var response = await client.PutAsJsonAsync("/api/roles/User/permissions", new { permissions = new[] { "Nope.Nothing" } });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("validation_failed", (await response.ReadJsonAsync()).GetProperty("code").GetString());
    }

    [Fact]
    public async Task UpdatePermissions_UnknownRole_Returns404()
    {
        var client = await AdminClientAsync();

        var response = await client.PutAsJsonAsync("/api/roles/Ghost/permissions", new { permissions = new[] { "Profile.View" } });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
