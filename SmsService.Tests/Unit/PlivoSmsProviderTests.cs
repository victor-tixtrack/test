using Moq;
using SmsService.Core.Interfaces;
using SmsService.Core.Models;
using SmsService.Infrastructure.Services;

namespace SmsService.Tests.Unit;

public class PlivoSmsProviderTests
{
    private readonly PlivoSmsProvider _provider;

    public PlivoSmsProviderTests()
    {
        _provider = new PlivoSmsProvider();
    }

    [Fact]
    public async Task SendSmsAsync_WithValidRequest_ReturnsSuccessResponse()
    {
        // Arrange
        var request = new SmsRequest { PhoneNumber = "+11234567890", Message = "Test SMS message" };

        // Act
        var response = await _provider.SendSmsAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Success);
        Assert.NotNull(response.MessageId);
        Assert.Null(response.ErrorMessage);
    }

    [Fact]
    public async Task SendSmsAsync_WithInvalidPhoneNumber_ReturnsErrorResponse()
    {
        // Arrange
        var request = new SmsRequest { PhoneNumber = "invalid", Message = "Test SMS message" };

        // Act
        var response = await _provider.SendSmsAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.False(response.Success);
        Assert.NotNull(response.ErrorCode);
        Assert.NotNull(response.ErrorMessage);
    }

    [Fact]
    public async Task SendSmsAsync_WithEmptyMessage_ReturnsErrorResponse()
    {
        // Arrange
        var request = new SmsRequest { PhoneNumber = "+11234567890", Message = "" };

        // Act
        var response = await _provider.SendSmsAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.False(response.Success);
        Assert.NotNull(response.ErrorCode);
    }

    [Fact]
    public async Task SendSmsAsync_WithMessageTooLong_ReturnsErrorResponse()
    {
        // Arrange
        var request = new SmsRequest
        {
            PhoneNumber = "+11234567890",
            Message = new string(
                'a',
                1601
            ) // Max is 1600
            ,
        };

        // Act
        var response = await _provider.SendSmsAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.False(response.Success);
        Assert.Equal("MESSAGE_TOO_LONG", response.ErrorCode);
    }

    [Fact]
    public async Task SendSmsAsync_WithValidE164Phone_Succeeds()
    {
        // Arrange
        var request = new SmsRequest
        {
            PhoneNumber = "+441234567890", // UK number in E.164
            Message = "Test message",
        };

        // Act
        var response = await _provider.SendSmsAsync(request);

        // Assert
        Assert.True(response.Success);
    }

    [Fact]
    public async Task SendSmsAsync_RespectsCancellationToken()
    {
        // Arrange
        var request = new SmsRequest { PhoneNumber = "+11234567890", Message = "Test message" };
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _provider.SendSmsAsync(request, cts.Token)
        );
    }
}
