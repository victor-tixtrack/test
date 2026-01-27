using SmsService.Domain.ValueObjects;

namespace SmsService.Domain.Entities;

/// <summary>
/// SMS consent tracking scoped to venue + phone number
/// </summary>
public class SmsConsent
{
    public int Id { get; set; }
    public int VenueId { get; set; }
    public required string PhoneNumberValue { get; set; }
    public int StatusId { get; set; }
    public int? ConsentSourceId { get; set; }
    public DateTime? InitialConsentAt { get; set; }
    public DateTime? OptedOutAt { get; set; }
    public DateTime? OptedInAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public SmsConsentStatus Status { get; set; } = null!;
    public ConsentSource? ConsentSource { get; set; }

    // Computed property for typed phone number
    public PhoneNumber PhoneNumber => PhoneNumber.Create(PhoneNumberValue);
}
