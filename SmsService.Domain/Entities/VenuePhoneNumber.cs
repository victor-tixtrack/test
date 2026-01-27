using SmsService.Domain.ValueObjects;

namespace SmsService.Domain.Entities;

/// <summary>
/// Phone number assignments per venue
/// </summary>
public class VenuePhoneNumber
{
    public int Id { get; set; }
    public int VenueId { get; set; }
    public required string PhoneNumberValue { get; set; }
    public required string ProviderId { get; set; }
    public int ProviderNameId { get; set; }
    public int StatusId { get; set; }
    public DateTime AssignedAt { get; set; }
    public DateTime? ReleasedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public ProviderName ProviderName { get; set; } = null!;
    public VenuePhoneNumberStatus Status { get; set; } = null!;
    public ICollection<SmsSendHistory> SmsSendHistories { get; set; } = new List<SmsSendHistory>();

    // Computed property for typed phone number
    public PhoneNumber PhoneNumber => PhoneNumber.Create(PhoneNumberValue);
}
