using Moq;
using SmsService.Api.Controllers;
using SmsService.Core.Interfaces;
using SmsService.Core.Models;
using Xunit;

namespace SmsService.Tests.Controllers;

public class SmsControllerTests
{
    [Fact]
    public async Task SendSms_CallsProviderWithRequest()
    {
        var mockProvider = new Mock<ISmsProvider>();
        var serviceResponse = new SmsResponse { Success = true, MessageId = "test-123" };
        mockProvider
            .Setup(p => p.SendSmsAsync(It.IsAny<SmsRequest>()))
            .ReturnsAsync(serviceResponse);

        var controller = new SmsController(mockProvider.Object);
        var request = new SmsRequest
        {
            PhoneNumber = "+1234567890",
            Message = "Test message",
        };
        var result = await controller.SendSms(request);

        Assert.Equal(result, serviceResponse);
    }
}
