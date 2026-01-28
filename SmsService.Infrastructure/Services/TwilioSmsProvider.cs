using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using SmsService.Core.Interfaces;
using SmsService.Core.Models;

namespace SmsService.Infrastructure.Services;

public class TwilioSmsProvider : ISmsProvider
{
    private readonly string? _accountSid;
    private readonly string? _authToken;
    private readonly string? _senderNumber;
    private readonly HttpClient _httpClient;
    private const string TwilioApiUrl = "https://api.twilio.com/2010-04-01";

    public TwilioSmsProvider(
        HttpClient httpClient,
        string? accountSid = null,
        string? authToken = null,
        string? senderNumber = null
    )
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _accountSid = accountSid ?? Environment.GetEnvironmentVariable("TWILIO_ACCOUNT_SID");
        _authToken = authToken ?? Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN");
        _senderNumber = senderNumber ?? Environment.GetEnvironmentVariable("TWILIO_SENDER_NUMBER");
    }

    public TwilioSmsProvider()
        : this(new HttpClient()) { }

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
            string.IsNullOrEmpty(_accountSid)
            || string.IsNullOrEmpty(_authToken)
            || string.IsNullOrEmpty(_senderNumber)
        )
        {
            return new SmsResponse
            {
                Success = false,
                ErrorCode = "MISSING_CREDENTIALS",
                ErrorMessage = "Twilio credentials not configured",
            };
        }

        try
        {
            // Create basic auth header
            var credentials = Convert.ToBase64String(
                Encoding.ASCII.GetBytes($"{_accountSid}:{_authToken}")
            );

            // Prepare request
            var requestUri = $"{TwilioApiUrl}/Accounts/{_accountSid}/Messages.json";
            var requestBody = new Dictionary<string, string>
            {
                { "From", _senderNumber },
                { "To", request.PhoneNumber },
                { "Body", request.Message },
            };

            if (!string.IsNullOrEmpty(request.CallbackUrl))
            {
                requestBody.Add("StatusCallback", request.CallbackUrl);
            }

            var content = new FormUrlEncodedContent(requestBody);

            // Create request message to avoid mutating shared HttpClient headers
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri)
            {
                Content = content,
            };
            requestMessage.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);

            // Send request
            var httpResponse = await _httpClient.SendAsync(requestMessage, cancellationToken);

            // Parse response
            var responseContent = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
            var parsedResponse = JsonConvert.DeserializeObject<TwilioResponse>(responseContent);

            if (httpResponse.IsSuccessStatusCode && parsedResponse != null)
            {
                return new SmsResponse { Success = true, MessageId = parsedResponse.Sid };
            }

            // Handle error response
            var errorMessage =
                parsedResponse?.Message ?? "Unknown error occurred while sending SMS";
            var errorCode = MapTwilioErrorCode(parsedResponse?.Code, httpResponse.StatusCode);

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
                ErrorMessage = $"Twilio provider error: {ex.Message}",
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

    private static string MapTwilioErrorCode(int? twilioCode, System.Net.HttpStatusCode statusCode)
    {
        // Twilio-specific error code mapping
        return twilioCode switch
        {
            21211 => "INVALID_PHONE", // Invalid 'To' Phone Number
            21408 => "OPTED_OUT", // Permission to send an SMS has not been enabled
            21610 => "OPTED_OUT", // Attempt to send to unsubscribed recipient
            30003 => "OPTED_OUT", // Unreachable destination handset
            30005 => "INVALID_PHONE", // Unknown destination handset
            30006 => "CARRIER_VIOLATION", // Landline or unreachable carrier
            30007 => "CARRIER_VIOLATION", // Carrier violation
            30008 => "INVALID_PHONE", // Unknown error
            _ => MapByStatusCode(statusCode),
        };
    }

    private static string MapByStatusCode(System.Net.HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            System.Net.HttpStatusCode.BadRequest => "INVALID_PHONE",
            System.Net.HttpStatusCode.Unauthorized => "PROVIDER_ERROR",
            System.Net.HttpStatusCode.Forbidden => "OPTED_OUT",
            System.Net.HttpStatusCode.NotFound => "PROVIDER_ERROR",
            System.Net.HttpStatusCode.TooManyRequests => "RATE_LIMITED",
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

    // Twilio API response model
    private class TwilioResponse
    {
        public string? Sid { get; set; }
        public string? Status { get; set; }
        public int? Code { get; set; }
        public string? Message { get; set; }
    }
}
