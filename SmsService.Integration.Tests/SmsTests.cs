using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SmsService.Core.Interfaces;
using SmsService.Core.Models;
using SmsService.Infrastructure.Services;
using Xunit;

namespace SmsService.Integration.Tests;

public class SmsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public SmsTests(WebApplicationFactory<Program> factory)
    {
        // Override the SMS provider with NoOp for testing
        var testFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d =>
                    d.ServiceType == typeof(ISmsProvider)
                );
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }
                services.AddScoped<ISmsProvider, NoOpSmsProvider>();
            });
        });

        _client = testFactory.CreateClient();
    }

    [Fact]
    public async Task SendSms_ValidRequest_ReturnsSuccess()
    {
        var request = new SmsRequest { PhoneNumber = "+1234567890", Message = "Test message" };

        var response = await _client.PostAsJsonAsync("/api/sms/send", request);

        response.EnsureSuccessStatusCode();
        var smsResponse = await response.Content.ReadFromJsonAsync<SmsResponse>();

        Assert.True(smsResponse.Success);
        Assert.StartsWith("noop-", smsResponse.MessageId);
    }
}
