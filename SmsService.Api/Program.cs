using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using SmsService.Api.Configuration;
using SmsService.Core.Interfaces;
using SmsService.Core.Models;
using SmsService.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services
builder.Services.AddHealthChecks();
builder.Services.ConfigureApiServices(builder.Configuration);

// Register Plivo provider
var plivoConfig = builder.Configuration.GetSection("Plivo");
builder.Services.AddSingleton<ISmsProvider>(
    new PlivoSmsProvider(
        plivoConfig["AuthId"],
        plivoConfig["AuthToken"],
        plivoConfig["SenderNumber"]
    )
);

var app = builder.Build();

// Configure middleware
app.MapHealthChecks(
    "/healthz/ready",
    new HealthCheckOptions { Predicate = healthCheck => healthCheck.Tags.Contains("ready") }
);

app.MapHealthChecks("/healthz/live", new HealthCheckOptions { Predicate = _ => false });

// Test SMS endpoint (temporary MVP - will be removed)
app.MapPost(
        "/api/test-sms",
        async (SmsRequest request, ISmsProvider smsProvider) =>
        {
            var response = await smsProvider.SendSmsAsync(request);
            return response.Success ? Results.Ok(response) : Results.BadRequest(response);
        }
    )
    .WithName("TestSendSms")
    .Produces<SmsResponse>(StatusCodes.Status200OK)
    .Produces<SmsResponse>(StatusCodes.Status400BadRequest);

app.Run();

Log.CloseAndFlush();

// Make Program class accessible to integration tests
public partial class Program { }
