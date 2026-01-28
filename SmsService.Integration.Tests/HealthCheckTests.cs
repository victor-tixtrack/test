using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace SmsService.Integration.Tests;

public class HealthCheckTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public HealthCheckTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task HealthCheck_Live_ReturnsHealthy()
    {
        var response = await _client.GetAsync("/healthz/live");

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal("Healthy", content);
    }
}
