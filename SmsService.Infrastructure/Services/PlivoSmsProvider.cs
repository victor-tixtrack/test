using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using SmsService.Core.Interfaces;
using SmsService.Core.Models;

namespace SmsService.Infrastructure.Services;

public class PlivoSmsProvider : ISmsProvider
{
    private readonly string? _authId;
    private readonly string? _authToken;
    private readonly string? _senderNumber;
    private readonly HttpClient _httpClient;
    private const string PlivoApiUrl = "https://api.plivo.com/v1";

    public PlivoSmsProvider(
        string? authId = null,
        string? authToken = null,
        string? senderNumber = null
    )
    {
        _authId = authId ?? Environment.GetEnvironmentVariable("PLIVO_AUTH_ID");
        _authToken = authToken ?? Environment.GetEnvironmentVariable("PLIVO_AUTH_TOKEN");
        _senderNumber = senderNumber ?? Environment.GetEnvironmentVariable("PLIVO_SENDER_NUMBER");
        _httpClient = new HttpClient();
    }

    public async Task<SmsResponse> SendSmsAsync(
        SmsRequest request,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var validationError = ValidateRequest(request);
        if (validationError != null)
        {
            return validationError;
        }

        if (
            string.IsNullOrEmpty(_authId)
            || string.IsNullOrEmpty(_authToken)
            || string.IsNullOrEmpty(_senderNumber)
        )
        {
            return new SmsResponse
            {
                Success = false,
                ErrorCode = "MISSING_CREDENTIALS",
                ErrorMessage = "Plivo credentials not configured",
            };
        }

        try
        {
            // Create basic auth header
            var credentials = Convert.ToBase64String(
                Encoding.ASCII.GetBytes($"{_authId}:{_authToken}")
            );

            // Prepare request
            var requestUri = $"{PlivoApiUrl}/Account/{_authId}/Message/";
            var content = new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    { "src", _senderNumber },
                    { "dst", request.PhoneNumber },
                    { "text", request.Message },
                }
            );

            // Set auth header
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);

            // Send request
            var httpResponse = await _httpClient.PostAsync(requestUri, content, cancellationToken);

            // Parse response
            var responseContent = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
            var parsedResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);

            if (httpResponse.IsSuccessStatusCode)
            {
                var messageUuid = parsedResponse?["message_uuid"]?[0]?.ToString();
                if (!string.IsNullOrEmpty(messageUuid))
                {
                    return new SmsResponse { Success = true, MessageId = messageUuid };
                }
            }

            // Handle error response
            var errorMessage = parsedResponse?["error"]?.ToString() ?? "Unknown error";
            var errorCode = MapPlivoErrorFromResponse(parsedResponse, httpResponse.StatusCode);

            return new SmsResponse
            {
                Success = false,
                ErrorCode = errorCode,
                ErrorMessage = errorMessage,
            };
        }
        catch (Exception ex)
        {
            return new SmsResponse
            {
                Success = false,
                ErrorCode = "PROVIDER_ERROR",
                ErrorMessage = $"Plivo provider error: {ex.Message}",
            };
        }
    }

    private static SmsResponse? ValidateRequest(SmsRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            return new SmsResponse
            {
                Success = false,
                ErrorCode = "INVALID_PHONE",
                ErrorMessage = "Phone number is required",
            };
        }

        if (!IsValidE164(request.PhoneNumber))
        {
            return new SmsResponse
            {
                Success = false,
                ErrorCode = "INVALID_PHONE",
                ErrorMessage = "Phone number must be in E.164 format (e.g., +1234567890)",
            };
        }

        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return new SmsResponse
            {
                Success = false,
                ErrorCode = "EMPTY_MESSAGE",
                ErrorMessage = "Message cannot be empty",
            };
        }

        if (request.Message.Length > 1600)
        {
            return new SmsResponse
            {
                Success = false,
                ErrorCode = "MESSAGE_TOO_LONG",
                ErrorMessage =
                    $"Message exceeds 1600 character limit (current length: {request.Message.Length})",
            };
        }

        return null;
    }

    private static string MapPlivoErrorFromResponse(
        dynamic response,
        System.Net.HttpStatusCode statusCode
    )
    {
        // Map based on status code
        return statusCode switch
        {
            System.Net.HttpStatusCode.BadRequest => "INVALID_PHONE",
            System.Net.HttpStatusCode.Unauthorized => "PROVIDER_ERROR",
            System.Net.HttpStatusCode.Forbidden => "OPTED_OUT",
            System.Net.HttpStatusCode.NotFound => "PROVIDER_ERROR",
            System.Net.HttpStatusCode.InternalServerError => "PROVIDER_ERROR",
            System.Net.HttpStatusCode.ServiceUnavailable => "PROVIDER_ERROR",
            _ => "PROVIDER_ERROR",
        };
    }

    private static bool IsValidE164(string phoneNumber)
    {
        var e164Regex = new Regex(@"^\+[1-9]\d{1,14}$");
        return e164Regex.IsMatch(phoneNumber);
    }
}
