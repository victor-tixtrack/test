using SmsService.Domain.Enums;

namespace SmsService.Domain.Entities;

public class SmsSendHistory
{
    public long Id { get; set; }
    public Guid OrderId { get; set; }
    public int VenueId { get; set; }
    public int VenuePhoneNumberId { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerPhoneNumber { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int SmsProviderId { get; set; }
    public SmsSendHistoryStatus Status { get; set; }
    public string? ProviderMessageId { get; set; }
    public string? ErrorCode { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public SmsProvider SmsProvider { get; set; } = null!;
}
