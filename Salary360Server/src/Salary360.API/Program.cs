using Salary360.API.Extensions;
using Salary360.API.Hubs;
using Salary360.IoC;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole();

builder.Services
    .AddSalary360()
    .AddApi(builder.Configuration);

var app = builder.Build();

await app.Services.InitializeDatabaseAsync();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "API v1");
        options.RoutePrefix = "swagger";
        options.EnablePersistAuthorization();
    });
    app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
}
app.UseHttpsRedirection();
app.UseCors(ApiServiceCollectionExtensions.CorsPolicyName);
app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationsHub>(NotificationsHub.Route);
app.MapHealthChecks("/health").AllowAnonymous();

app.Run();

public partial class Program;
