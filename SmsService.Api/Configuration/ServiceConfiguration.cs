using Microsoft.EntityFrameworkCore;
using SmsService.Core.Interfaces;
using SmsService.Domain.Data;

namespace SmsService.Api.Configuration;

public static class ServiceConfiguration
{
    /// <summary>
    /// Configures API services including health checks, logging, and dependency injection.
    /// </summary>
    public static IServiceCollection ConfigureApiServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // Add health checks
        services.AddHealthChecks();

        // Register controllers
        services.AddControllers();

        // Register DbContext
        services.AddDbContext<SmsDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("SmsDatabase"),
                sqlOptions =>
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorNumbersToAdd: null
                    )
            )
        );

        // Auto-register services using Scrutor (excluding providers that need configuration)
        services.Scan(scan =>
            scan.FromApplicationDependencies()
                .AddClasses(classes =>
                    classes.AssignableTo<ISmsProvider>().Where(type => !type.Name.Contains("Plivo"))
                )
                .AsImplementedInterfaces()
                .WithScopedLifetime()
        );

        return services;
    }
}
