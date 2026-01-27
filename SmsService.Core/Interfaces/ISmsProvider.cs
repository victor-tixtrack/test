namespace SmsService.Core.Interfaces;

using SmsService.Core.Models;

public interface ISmsProvider
{
    /// <summary>
    /// Sends an SMS message asynchronously.
    /// </summary>
    /// <param name="request">The SMS request containing phone number, message, and metadata</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>SMS response with success status, message ID, and error details if applicable</returns>
    Task<SmsResponse> SendSmsAsync(
        SmsRequest request,
        CancellationToken cancellationToken = default
    );
}
