using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Salary360.API.Hubs;
using Salary360.API.Interfaces;
using Salary360.API.Middleware;
using Salary360.API.Services;
using Salary360.API.Settings;
using Salary360.Application.Common;
using Salary360.Application.Interfaces.Identity;
using Salary360.Application.Interfaces.Notifications;
using Salary360.Application.Settings;

namespace Salary360.API.Extensions;

public static class ApiServiceCollectionExtensions
{
    public const string CorsPolicyName = "Client";
    public const string LoginRateLimitPolicy = "login";
    public const string RegisterRateLimitPolicy = "register";

    public static IServiceCollection AddApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers()
            .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        services.AddOpenApi(options => options.AddDocumentTransformer((document, context, _) =>
        {
            var project = context.ApplicationServices.GetRequiredService<IOptions<ProjectSettings>>().Value;
            document.Info.Title = project.Name;
            document.Info.Description = project.Description;

            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>
            {
                ["Bearer"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "Paste the JWT returned by POST /api/auth/login."
                }
            };
            document.Security =
            [
                new OpenApiSecurityRequirement { [new OpenApiSecuritySchemeReference("Bearer", document)] = [] }
            ];

            return Task.CompletedTask;
        }));
        services.AddHealthChecks();

        services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
        {
            if (context.ProblemDetails.Extensions.ContainsKey("code"))
                return;

            context.ProblemDetails.Extensions["code"] = (context.ProblemDetails.Status ?? context.HttpContext.Response.StatusCode) switch
            {
                StatusCodes.Status400BadRequest => ErrorCodes.ValidationFailed,
                StatusCodes.Status401Unauthorized => ErrorCodes.Unauthorized,
                StatusCodes.Status403Forbidden => ErrorCodes.Forbidden,
                StatusCodes.Status404NotFound => ErrorCodes.NotFound,
                StatusCodes.Status429TooManyRequests => ErrorCodes.TooManyRequests,
                _ => ErrorCodes.InternalError
            };
        });
        services.AddExceptionHandler<GlobalExceptionHandler>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();

        services.AddSignalR();
        services.AddSingleton<IUserIdProvider, SubjectUserIdProvider>();
        services.AddSingleton<INotificationPublisher, SignalRNotificationPublisher>();

        services.AddJwtAuthentication();
        services.AddAuthorization(options =>
        {
            foreach (var permission in AppPermissions.All)
                options.AddPolicy(permission, policy => policy.RequireClaim(AppPermissions.ClaimType, permission));
        });

        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        services.AddCors(options => options.AddPolicy(CorsPolicyName, policy => policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()));

        services.AddOptions<RateLimitingSettings>()
            .BindConfiguration(RateLimitingSettings.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton<ILoginAttemptLimiter, LoginAttemptLimiter>();

        services.AddRateLimiter(_ => { });
        services.AddOptions<RateLimiterOptions>().Configure<IOptions<RateLimitingSettings>>((options, rateLimiting) =>
        {
            var settings = rateLimiting.Value;

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    httpContext.User.FindFirst("sub")?.Value ?? ClientIp(httpContext),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = settings.ApiPermitLimit,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));

            options.AddPolicy(LoginRateLimitPolicy, httpContext => RateLimitPartition.GetFixedWindowLimiter(
                ClientIp(httpContext),
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = settings.LoginIpPermitLimit,
                    Window = TimeSpan.FromMinutes(settings.LoginWindowMinutes),
                    QueueLimit = 0
                }));

            options.AddPolicy(RegisterRateLimitPolicy, httpContext => RateLimitPartition.GetFixedWindowLimiter(
                ClientIp(httpContext),
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = settings.RegisterPermitLimit,
                    Window = TimeSpan.FromMinutes(settings.RegisterWindowMinutes),
                    QueueLimit = 0
                }));

            options.OnRejected = async (context, cancellationToken) =>
            {
                var retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var value)
                    ? value
                    : TimeSpan.FromMinutes(1);
                var http = context.HttpContext;
                var seconds = ApiProblem.SecondsOf(retryAfter);

                http.RequestServices.GetRequiredService<ILoggerFactory>()
                    .CreateLogger("RateLimiting")
                    .LogWarning("Request rejected by the rate limiter on {Path}", http.Request.Path);

                http.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                http.Response.Headers.RetryAfter = seconds.ToString(System.Globalization.CultureInfo.InvariantCulture);

                await http.RequestServices.GetRequiredService<IProblemDetailsService>().TryWriteAsync(new ProblemDetailsContext
                {
                    HttpContext = http,
                    ProblemDetails = ApiProblem.WithRetryAfter(
                        ApiProblem.Create(StatusCodes.Status429TooManyRequests, "Too many requests.", ErrorCodes.TooManyRequests),
                        retryAfter)
                });
            };
        });

        return services;
    }

    private static string ClientIp(HttpContext httpContext)
        => httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    private static IServiceCollection AddJwtAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtSettings>>((bearer, jwt) =>
            {
                var settings = jwt.Value;
                bearer.MapInboundClaims = false;
                bearer.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = settings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = settings.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Secret)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1),
                    NameClaimType = "name",
                    RoleClaimType = "role"
                };
                bearer.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        if (!string.IsNullOrEmpty(accessToken) && context.HttpContext.Request.Path.StartsWithSegments(NotificationsHub.Route))
                            context.Token = accessToken;

                        return Task.CompletedTask;
                    },
                    OnTokenValidated = async context =>
                    {
                        var subject = context.Principal?.FindFirst("sub")?.Value;
                        var validator = context.HttpContext.RequestServices.GetRequiredService<IUserSessionValidator>();

                        if (!Guid.TryParse(subject, out var userId) ||
                            !await validator.IsActiveAsync(userId, context.HttpContext.RequestAborted))
                            context.Fail("The user no longer exists or is inactive.");
                    }
                };
            });

        return services;
    }
}
