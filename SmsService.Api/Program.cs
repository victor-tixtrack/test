using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using SmsService.Api.Configuration;

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

var app = builder.Build();

// Configure middleware
app.MapHealthChecks(
    "/healthz/ready",
    new HealthCheckOptions { Predicate = healthCheck => healthCheck.Tags.Contains("ready") }
);

app.MapHealthChecks("/healthz/live", new HealthCheckOptions { Predicate = _ => false });

app.Run();

Log.CloseAndFlush();
