using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using FluentValidation;
using Salary360.Application.Common;
using Salary360.Application.Common.Interfaces;
using Salary360.Application.Expenses.Interfaces;
using Salary360.Application.Expenses.Services;
using Salary360.Application.Interfaces.Email;
using Salary360.Application.Interfaces.Identity;
using Salary360.Application.Interfaces.Messaging;
using Salary360.Application.Interfaces.Persistence;
using Salary360.Application.SalaryIntelligence.Interfaces;
using Salary360.Application.SalaryIntelligence.Services;
using Salary360.Application.Settings;
using Salary360.Domain.Interfaces;
using Salary360.Infrastructure.Email;
using Salary360.Infrastructure.Events;
using Salary360.Infrastructure.Identity;
using Salary360.Infrastructure.Inflation;
using Salary360.Infrastructure.Interfaces;
using Salary360.Infrastructure.Persistence;
using Salary360.Infrastructure.Seed;
using Salary360.Infrastructure.Seed.Interfaces;

namespace Salary360.IoC;

public static class DependencyInjection
{
    public static IServiceCollection AddSalary360(this IServiceCollection services)
        => services
            .AddApplicationServices()
            .AddInfrastructureServices();

    public static async Task InitializeDatabaseAsync(this IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        await DatabaseInitializer.InitializeAsync(serviceProvider, cancellationToken);
        await IdentitySeeder.SeedAsync(serviceProvider, cancellationToken);
        await SampleDataSeedOrchestrator.SeedAsync(serviceProvider, cancellationToken);
    }

    private static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var applicationAssembly = typeof(IDispatcher).Assembly;

        services.AddOptions<ProjectSettings>().BindConfiguration(ProjectSettings.SectionName);

        services.AddScoped<IDispatcher, Dispatcher>();
        services.AddValidatorsFromAssembly(applicationAssembly);

        var handlerContract = typeof(IRequestHandler<,>);
        var handlers = applicationAssembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false })
            .SelectMany(type => type.GetInterfaces()
                .Where(contract => contract.IsGenericType && contract.GetGenericTypeDefinition() == handlerContract)
                .Select(contract => (Contract: contract, Implementation: type)));

        foreach (var (contract, implementation) in handlers)
            services.AddScoped(contract, implementation);

        services.AddSingleton<ISalaryEvolutionCalculator, SalaryEvolutionCalculator>();
        services.AddSingleton<IPurchasingPowerCalculator, PurchasingPowerCalculator>();
        services.AddSingleton<ISalaryInsightGenerator, SalaryInsightGenerator>();
        services.AddSingleton<ISalaryProjectionCalculator, SalaryProjectionCalculator>();
        services.AddScoped<IDefaultExpenseCategoryService, DefaultExpenseCategoryService>();

        return services;
    }

    private static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);

        services.AddOptions<JwtSettings>()
            .BindConfiguration(JwtSettings.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<ClientSettings>()
            .BindConfiguration(ClientSettings.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<EmailSettings>()
            .BindConfiguration(EmailSettings.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<IbgeSettings>()
            .BindConfiguration(IbgeSettings.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<SeedSettings>().BindConfiguration(SeedSettings.SectionName);

        services.AddScoped<ISampleDataSeeder, SalarySampleDataSeeder>();
        services.AddScoped<ISampleDataSeeder, ExpensesSampleDataSeeder>();

        services.AddMemoryCache();
        services.AddHttpClient(IbgeInflationProvider.HttpClientName, (sp, client) =>
        {
            var ibge = sp.GetRequiredService<IOptions<IbgeSettings>>().Value;
            client.BaseAddress = new Uri(ibge.BaseUrl.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(ibge.TimeoutSeconds);
        });
        services.AddSingleton<IInflationProvider, IbgeInflationProvider>();
        services.AddSingleton<IEmailTemplateRenderer, EmbeddedEmailTemplateRenderer>();
        services.AddScoped<IEmailSender>(serviceProvider =>
        {
            var provider = serviceProvider.GetRequiredService<IOptions<EmailSettings>>().Value.Provider;
            return string.Equals(provider, "Log", StringComparison.OrdinalIgnoreCase)
                ? ActivatorUtilities.CreateInstance<LoggingEmailSender>(serviceProvider)
                : ActivatorUtilities.CreateInstance<SmtpEmailSender>(serviceProvider);
        });
        services.AddScoped<ConfirmationEmailService>();

        services.AddDbContext<AppDbContext>((serviceProvider, options) =>
        {
            var connectionString = serviceProvider.GetRequiredService<IConfiguration>().GetConnectionString("DefaultConnection")
                                   ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
            options.UseSqlite(connectionString);
        });

        services.AddScoped<IUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<AppDbContext>());
        services.AddScoped<ISalaryRecordRepository, SalaryRecordRepository>();
        services.AddScoped<IExpenseCategoryRepository, ExpenseCategoryRepository>();
        services.AddScoped<IExpenseRepository, ExpenseRepository>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 3;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        services.AddDataProtection();
        services.Configure<DataProtectionTokenProviderOptions>(options => options.TokenLifespan = TimeSpan.FromHours(24));

        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IUserSessionValidator, UserSessionValidator>();

        return services;
    }
}