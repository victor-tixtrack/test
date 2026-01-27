namespace SmsService.Domain.Entities;

/// <summary>
/// Enum table for consent source (checkout, account_settings, support_request)
/// </summary>
public class ConsentSource
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public ICollection<SmsConsent> SmsConsents { get; set; } = new List<SmsConsent>();
}
