using System.Net;
using FluentAssertions;
using Moq;
using Moq.Protected;
using SmsService.Core.Models;
using SmsService.Infrastructure.Services;
using Xunit;

namespace SmsService.Tests.Unit;

public class TwilioSmsProviderTests
{
    private static HttpClient CreateMockHttpClient(HttpResponseMessage response)
    {
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(response);

        return new HttpClient(mockHandler.Object);
    }

    [Fact]
    public async Task SendSmsAsync_WithMissingCredentials_ReturnsError()
    {
        // Arrange
        var httpClient = CreateMockHttpClient(new HttpResponseMessage(HttpStatusCode.OK));
        var provider = new TwilioSmsProvider(httpClient);
        var request = new SmsRequest { PhoneNumber = "+11234567890", Message = "Test message" };

        // Act
        var result = await provider.SendSmsAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("MISSING_CREDENTIALS");
    }

    [Fact]
    public async Task SendSmsAsync_WithEmptyPhoneNumber_ReturnsError()
    {
        // Arrange
        var httpClient = CreateMockHttpClient(new HttpResponseMessage(HttpStatusCode.OK));
        var provider = new TwilioSmsProvider(httpClient, "test-sid", "test-token", "+11234567890");
        var request = new SmsRequest { PhoneNumber = "", Message = "Test message" };

        // Act
        var result = await provider.SendSmsAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_PHONE");
        result.ErrorMessage.Should().Contain("required");
    }

    [Fact]
    public async Task SendSmsAsync_WithInvalidE164Format_ReturnsError()
    {
        // Arrange
        var httpClient = CreateMockHttpClient(new HttpResponseMessage(HttpStatusCode.OK));
        var provider = new TwilioSmsProvider(httpClient, "test-sid", "test-token", "+11234567890");
        var request = new SmsRequest { PhoneNumber = "1234567890", Message = "Test message" };

        // Act
        var result = await provider.SendSmsAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_PHONE");
        result.ErrorMessage.Should().Contain("E.164");
    }

    [Theory]
    [InlineData("+0123456789")] // Cannot start with 0
    [InlineData("+1")] // Too short
    [InlineData("+123456789012345678")] // Too long (>15 digits)
    [InlineData("1234567890")] // Missing +
    [InlineData("+1-234-567-890")] // Contains hyphens
    [InlineData("+1 234 567 890")] // Contains spaces
    public async Task SendSmsAsync_WithInvalidE164Formats_ReturnsError(string invalidPhone)
    {
        // Arrange
        var httpClient = CreateMockHttpClient(new HttpResponseMessage(HttpStatusCode.OK));
        var provider = new TwilioSmsProvider(httpClient, "test-sid", "test-token", "+11234567890");
        var request = new SmsRequest { PhoneNumber = invalidPhone, Message = "Test message" };

        // Act
        var result = await provider.SendSmsAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_PHONE");
    }

    [Theory]
    [InlineData("+12345678901")] // US number
    [InlineData("+442071234567")] // UK number
    [InlineData("+61412345678")] // AU number
    [InlineData("+81312345678")] // JP number
    public async Task SendSmsAsync_WithValidE164Formats_PassesValidation(string validPhone)
    {
        // Arrange
        var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(@"{""sid"": ""SM123"", ""status"": ""queued""}"),
        };
        var httpClient = CreateMockHttpClient(mockResponse);
        var provider = new TwilioSmsProvider(httpClient, "test-sid", "test-token", "+11234567890");
        var request = new SmsRequest { PhoneNumber = validPhone, Message = "Test message" };

        // Act
        var result = await provider.SendSmsAsync(request);

        // Assert - successfully processes without INVALID_PHONE error
        result.ErrorCode.Should().NotBe("INVALID_PHONE");
        result.Success.Should().BeTrue();
        result.MessageId.Should().Be("SM123");
    }

    [Fact]
    public async Task SendSmsAsync_WithEmptyMessage_ReturnsError()
    {
        // Arrange
        var httpClient = CreateMockHttpClient(new HttpResponseMessage(HttpStatusCode.OK));
        var provider = new TwilioSmsProvider(httpClient, "test-sid", "test-token", "+11234567890");
        var request = new SmsRequest { PhoneNumber = "+11234567890", Message = "" };

        // Act
        var result = await provider.SendSmsAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("EMPTY_MESSAGE");
        result.ErrorMessage.Should().Contain("empty");
    }

    [Fact]
    public async Task SendSmsAsync_WithMessageTooLong_ReturnsError()
    {
        // Arrange
        var httpClient = CreateMockHttpClient(new HttpResponseMessage(HttpStatusCode.OK));
        var provider = new TwilioSmsProvider(httpClient, "test-sid", "test-token", "+11234567890");
        var longMessage = new string('X', 1601);
        var request = new SmsRequest { PhoneNumber = "+11234567890", Message = longMessage };

        // Act
        var result = await provider.SendSmsAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("MESSAGE_TOO_LONG");
        result.ErrorMessage.Should().Contain("1600");
        result.ErrorMessage.Should().Contain("1601");
    }

    [Fact]
    public async Task SendSmsAsync_WithMaxLengthMessage_PassesValidation()
    {
        // Arrange
        var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(@"{""sid"": ""SM123"", ""status"": ""queued""}"),
        };
        var httpClient = CreateMockHttpClient(mockResponse);
        var provider = new TwilioSmsProvider(httpClient, "test-sid", "test-token", "+11234567890");
        var maxLengthMessage = new string('X', 1600);
        var request = new SmsRequest { PhoneNumber = "+11234567890", Message = maxLengthMessage };

        // Act
        var result = await provider.SendSmsAsync(request);

        // Assert - successfully processes without MESSAGE_TOO_LONG error
        result.ErrorCode.Should().NotBe("MESSAGE_TOO_LONG");
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task SendSmsAsync_WithCancellationToken_ThrowsWhenCancelled()
    {
        // Arrange
        var httpClient = CreateMockHttpClient(new HttpResponseMessage(HttpStatusCode.OK));
        var provider = new TwilioSmsProvider(httpClient, "test-sid", "test-token", "+11234567890");
        var request = new SmsRequest { PhoneNumber = "+11234567890", Message = "Test message" };
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await provider.SendSmsAsync(request, cts.Token)
        );
    }

    [Fact]
    public async Task SendSmsAsync_WithSuccessfulTwilioResponse_ReturnsSuccess()
    {
        // Arrange
        var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(@"{""sid"": ""SM123456789"", ""status"": ""queued""}"),
        };
        var httpClient = CreateMockHttpClient(mockResponse);
        var provider = new TwilioSmsProvider(httpClient, "test-sid", "test-token", "+11234567890");
        var request = new SmsRequest { PhoneNumber = "+12345678901", Message = "Test message" };

        // Act
        var result = await provider.SendSmsAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.MessageId.Should().Be("SM123456789");
        result.ErrorCode.Should().BeNull();
    }

    [Fact]
    public async Task SendSmsAsync_WithTwilioErrorResponse_ReturnsError()
    {
        // Arrange
        var mockResponse = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(
                @"{""code"": 21211, ""message"": ""Invalid phone number""}"
            ),
        };
        var httpClient = CreateMockHttpClient(mockResponse);
        var provider = new TwilioSmsProvider(httpClient, "test-sid", "test-token", "+11234567890");
        var request = new SmsRequest { PhoneNumber = "+12345678901", Message = "Test message" };

        // Act
        var result = await provider.SendSmsAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_PHONE");
        result.ErrorMessage.Should().Contain("Invalid phone number");
    }
}
