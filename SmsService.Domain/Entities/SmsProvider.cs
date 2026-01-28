namespace SmsService.Domain.Entities;

/// <summary>
/// SMS provider entity (Twilio, Plivo, etc.)
/// </summary>
public class SmsProvider
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}
