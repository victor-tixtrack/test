using Microsoft.EntityFrameworkCore;
using SmsService.Core.Interfaces;
using Newtonsoft.Json.Converters;
using SmsService.Domain.Data;
using SmsService.Infrastructure.Services;

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
        services
            .AddControllers()
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.Converters.Add(new StringEnumConverter());
            });

        // Add health checks
        services.AddHealthChecks();

        // Register NoOp SMS Provider for testing
        services.AddScoped<ISmsProvider, NoOpSmsProvider>();

        // Register SMS Provider with HttpClient (commented out)
        // services.AddHttpClient();
        // services.AddScoped<ISmsProvider>(serviceProvider =>
        // {
        //     var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        //     var httpClient = httpClientFactory.CreateClient();
        //
        //     var accountSid = configuration["Twilio:AccountSid"];
        //     var authToken = configuration["Twilio:AuthToken"];
        //     var senderNumber = configuration["Twilio:SenderNumber"];
        //
        //     return new TwilioSmsProvider(httpClient, accountSid, authToken, senderNumber);
        // });

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

        return services;
    }
}
