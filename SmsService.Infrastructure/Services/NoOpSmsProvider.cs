using SmsService.Core.Interfaces;
using SmsService.Core.Models;

namespace SmsService.Infrastructure.Services;

public class NoOpSmsProvider : ISmsProvider
{
    public Task<SmsResponse> SendSmsAsync(
        SmsRequest request,
        CancellationToken cancellationToken = default
    )
    {
        // No-op SMS provider implementation
        var response = new SmsResponse
        {
            Success = true,
            MessageId = $"noop-{Guid.NewGuid()}",
            ErrorCode = null,
            ErrorMessage = null,
        };

        return Task.FromResult(response);
    }
}
