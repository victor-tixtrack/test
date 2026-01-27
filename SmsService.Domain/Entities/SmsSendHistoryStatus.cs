namespace SmsService.Domain.Entities;

/// <summary>
/// Enum table for SMS send history status (sent, failed, skipped_no_consent, blocked_opted_out)
/// </summary>
public class SmsSendHistoryStatus
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public ICollection<SmsSendHistory> SmsSendHistories { get; set; } = new List<SmsSendHistory>();
}
