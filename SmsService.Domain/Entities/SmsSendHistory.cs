using SmsService.Domain.ValueObjects;

namespace SmsService.Domain.Entities;

/// <summary>
/// SMS send history for audit and compliance tracking
/// </summary>
public class SmsSendHistory
{
    public long Id { get; set; }
    public Guid OrderId { get; set; }
    public int VenueId { get; set; }
    public int VenuePhoneNumberId { get; set; }
    public Guid CustomerId { get; set; }
    public required string CustomerPhoneNumberValue { get; set; }
    public required string Message { get; set; }
    public int ProviderNameId { get; set; }
    public int StatusId { get; set; }
    public string? ProviderMessageId { get; set; }
    public string? ErrorCode { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public VenuePhoneNumber VenuePhoneNumber { get; set; } = null!;
    public ProviderName ProviderName { get; set; } = null!;
    public SmsSendHistoryStatus Status { get; set; } = null!;

    // Computed property for typed phone number
    public PhoneNumber CustomerPhoneNumber => PhoneNumber.Create(CustomerPhoneNumberValue);
}
