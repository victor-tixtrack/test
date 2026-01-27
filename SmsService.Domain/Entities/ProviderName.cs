namespace SmsService.Domain.Entities;

/// <summary>
/// Enum table for SMS provider names (Twilio, Plivo, etc.)
/// </summary>
public class ProviderName
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}
