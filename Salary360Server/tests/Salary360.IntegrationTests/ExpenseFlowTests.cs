using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Salary360.Domain.Entities;
using Salary360.IntegrationTests.Support;

namespace Salary360.IntegrationTests;

public sealed class ExpenseFlowTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory factory;

    public ExpenseFlowTests(ApiFactory factory) => this.factory = factory;

    [Fact]
    public async Task CategoryAndExpense_Crud_Conflicts_Validation_Summary_AndIsolation()
    {
        var client = factory.CreateClient();
        var userToken = await client.LoginAsync(ApiFactory.UserEmail, ApiFactory.UserPassword);
        var adminToken = await client.LoginAsync(ApiFactory.AdminEmail, ApiFactory.AdminPassword);
        var user = factory.CreateClient().WithToken(userToken);
        var admin = factory.CreateClient().WithToken(adminToken);

        const int year = 2021;
        const int month = 8;

        var salary = await user.PostAsJsonAsync("/api/salaries", new
        {
            year,
            month,
            grossAmount = 5000m,
            notes = "Expense summary salary",
            items = new[]
            {
                new { name = "VR", type = "Benefit", amount = 400m },
                new { name = "INSS", type = "Deduction", amount = 500m }
            }
        });
        Assert.Equal(HttpStatusCode.Created, salary.StatusCode);

        var createCategory = await user.PostAsJsonAsync("/api/expense-categories", new
        {
            name = $"Travel-{Guid.NewGuid():N}"[..20],
            colorKey = "Violet",
            iconKey = "Truck"
        });
        Assert.Equal(HttpStatusCode.Created, createCategory.StatusCode);
        var category = await createCategory.ReadJsonAsync();
        var categoryId = category.GetProperty("id").GetGuid();

        var duplicateCategory = await user.PostAsJsonAsync("/api/expense-categories", new
        {
            name = category.GetProperty("name").GetString(),
            colorKey = "Blue",
            iconKey = "Home"
        });
        Assert.Equal(HttpStatusCode.Conflict, duplicateCategory.StatusCode);
        Assert.Equal("duplicate_category_name", (await duplicateCategory.ReadJsonAsync()).GetProperty("code").GetString());

        var createExpense = await user.PostAsJsonAsync("/api/expenses", new
        {
            categoryId,
            name = "Weekend trip",
            type = "Variable",
            amount = 350.55m,
            year,
            month,
            dueDay = 15,
            isPaid = true,
            notes = "First expense",
            recurrence = "None",
            recurrenceCount = (int?)null
        });
        Assert.Equal(HttpStatusCode.Created, createExpense.StatusCode);
        var expenses = await createExpense.ReadJsonAsync();
        Assert.Equal(JsonValueKind.Array, expenses.ValueKind);
        var expenseId = expenses[0].GetProperty("id").GetGuid();
        Assert.Equal(350.55m, expenses[0].GetProperty("amount").GetDecimal());

        var invalid = await user.PostAsJsonAsync("/api/expenses", new
        {
            categoryId,
            name = "",
            type = "Variable",
            amount = 0m,
            year = 1800,
            month = 13,
            dueDay = 40,
            isPaid = false,
            notes = new string('n', 501),
            recurrence = "None",
            recurrenceCount = (int?)null
        });
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        Assert.True((await invalid.ReadJsonAsync()).TryGetProperty("errors", out _));

        var update = await user.PutAsJsonAsync($"/api/expenses/{expenseId}?scope=This", new
        {
            categoryId,
            name = "Weekend trip updated",
            type = "Variable",
            amount = 400m,
            year,
            month,
            dueDay = 16,
            isPaid = false,
            notes = "Updated"
        });
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        Assert.Equal(400m, (await update.ReadJsonAsync()).GetProperty("amount").GetDecimal());

        var list = await user.GetAsync($"/api/expenses?year={year}&month={month}&page=1&pageSize=20");
        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        Assert.True((await list.ReadJsonAsync()).GetProperty("total").GetInt32() >= 1);

        var summary = await user.GetAsync($"/api/expenses/summary?year={year}&month={month}");
        Assert.Equal(HttpStatusCode.OK, summary.StatusCode);
        var summaryBody = await summary.ReadJsonAsync();
        Assert.True(summaryBody.GetProperty("hasSalary").GetBoolean());
        Assert.Equal(4900m, summaryBody.GetProperty("netAmount").GetDecimal());
        Assert.True(summaryBody.GetProperty("total").GetDecimal() >= 400m);
        Assert.NotEqual(JsonValueKind.Null, summaryBody.GetProperty("available").ValueKind);

        var foreignGet = await admin.GetAsync($"/api/expenses/{expenseId}");
        Assert.Equal(HttpStatusCode.NotFound, foreignGet.StatusCode);

        var foreignCategory = await admin.PostAsJsonAsync("/api/expense-categories", new
        {
            name = $"AdminCat-{Guid.NewGuid():N}"[..20],
            colorKey = "Red",
            iconKey = "Heart"
        });
        Assert.Equal(HttpStatusCode.Created, foreignCategory.StatusCode);
        var adminCategoryId = (await foreignCategory.ReadJsonAsync()).GetProperty("id").GetGuid();

        var useForeignCategory = await user.PostAsJsonAsync("/api/expenses", new
        {
            categoryId = adminCategoryId,
            name = "Should fail",
            type = "Fixed",
            amount = 10m,
            year,
            month,
            dueDay = (int?)null,
            isPaid = false,
            notes = (string?)null,
            recurrence = "None",
            recurrenceCount = (int?)null
        });
        Assert.Equal(HttpStatusCode.NotFound, useForeignCategory.StatusCode);

        var secondCategory = await user.PostAsJsonAsync("/api/expense-categories", new
        {
            name = $"Backup-{Guid.NewGuid():N}"[..20],
            colorKey = "Teal",
            iconKey = "CreditCard"
        });
        Assert.Equal(HttpStatusCode.Created, secondCategory.StatusCode);
        var secondCategoryId = (await secondCategory.ReadJsonAsync()).GetProperty("id").GetGuid();

        var deleteInUse = await user.DeleteAsync($"/api/expense-categories/{categoryId}");
        Assert.Equal(HttpStatusCode.Conflict, deleteInUse.StatusCode);
        Assert.Equal("category_in_use", (await deleteInUse.ReadJsonAsync()).GetProperty("code").GetString());

        var reassign = await user.DeleteAsync($"/api/expense-categories/{categoryId}?reassignTo={secondCategoryId}");
        Assert.Equal(HttpStatusCode.NoContent, reassign.StatusCode);

        var moved = await user.GetAsync($"/api/expenses/{expenseId}");
        Assert.Equal(HttpStatusCode.OK, moved.StatusCode);
        Assert.Equal(secondCategoryId, (await moved.ReadJsonAsync()).GetProperty("categoryId").GetGuid());

        var deleteExpense = await user.DeleteAsync($"/api/expenses/{expenseId}?scope=This");
        Assert.Equal(HttpStatusCode.NoContent, deleteExpense.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await user.GetAsync($"/api/expenses/{expenseId}")).StatusCode);

        await user.DeleteAsync($"/api/expense-categories/{secondCategoryId}");
    }

    [Fact]
    public async Task TooManyExpensesInMonth_ReturnsConflict()
    {
        var token = await factory.CreateClient().LoginAsync(ApiFactory.UserEmail, ApiFactory.UserPassword);
        var user = factory.CreateClient().WithToken(token);

        var categoryResponse = await user.PostAsJsonAsync("/api/expense-categories", new
        {
            name = $"Limit-{Guid.NewGuid():N}"[..20],
            colorKey = "Slate",
            iconKey = "EllipsisHorizontal"
        });
        Assert.Equal(HttpStatusCode.Created, categoryResponse.StatusCode);
        var categoryId = (await categoryResponse.ReadJsonAsync()).GetProperty("id").GetGuid();

        var year = 2019;
        var month = 4;

        for (var index = 0; index < Expense.MaxPerUserPerMonth; index++)
        {
            var response = await user.PostAsJsonAsync("/api/expenses", new
            {
                categoryId,
                name = $"Item {index}",
                type = "Variable",
                amount = 1.01m,
                year,
                month,
                dueDay = (int?)null,
                isPaid = false,
                notes = (string?)null,
                recurrence = "None",
                recurrenceCount = (int?)null
            });
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        var overflow = await user.PostAsJsonAsync("/api/expenses", new
        {
            categoryId,
            name = "Overflow",
            type = "Variable",
            amount = 1.01m,
            year,
            month,
            dueDay = (int?)null,
            isPaid = false,
            notes = (string?)null,
            recurrence = "None",
            recurrenceCount = (int?)null
        });
        Assert.Equal(HttpStatusCode.Conflict, overflow.StatusCode);
        Assert.Equal("too_many_expenses_in_month", (await overflow.ReadJsonAsync()).GetProperty("code").GetString());
    }

    [Fact]
    public async Task UnauthorizedAndForbidden()
    {
        var anonymous = factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync("/api/expenses?year=2024&month=1")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync("/api/expense-categories")).StatusCode);

        var adminToken = await factory.CreateClient().LoginAsync(ApiFactory.AdminEmail, ApiFactory.AdminPassword);
        var admin = factory.CreateClient().WithToken(adminToken);

        var strip = await admin.PutAsJsonAsync("/api/roles/User/permissions", new
        {
            permissions = new[] { "Profile.View", "Profile.Update" }
        });
        Assert.Equal(HttpStatusCode.OK, strip.StatusCode);

        var userToken = await factory.CreateClient().LoginAsync(ApiFactory.UserEmail, ApiFactory.UserPassword);
        var forbidden = await factory.CreateClient().WithToken(userToken).GetAsync("/api/expenses?year=2024&month=1");
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
