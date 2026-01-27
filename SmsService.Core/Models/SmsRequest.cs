namespace SmsService.Core.Models;

public class SmsRequest
{
    public required string PhoneNumber { get; set; }
    public required string Message { get; set; }
    public string? CallbackUrl { get; set; }
    public int? VenueId { get; set; }
}

public class SmsResponse
{
    public bool Success { get; set; }
    public string? MessageId { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
}
