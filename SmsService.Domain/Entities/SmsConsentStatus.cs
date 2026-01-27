namespace SmsService.Domain.Entities;

/// <summary>
/// Enum table for SMS consent status (opted_in, opted_out)
/// </summary>
public class SmsConsentStatus
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public ICollection<SmsConsent> SmsConsents { get; set; } = new List<SmsConsent>();
}
