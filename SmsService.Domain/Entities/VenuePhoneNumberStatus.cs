namespace SmsService.Domain.Entities;

/// <summary>
/// Enum table for venue phone number status (active, inactive, released)
/// </summary>
public class VenuePhoneNumberStatus
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public ICollection<VenuePhoneNumber> VenuePhoneNumbers { get; set; } =
        new List<VenuePhoneNumber>();
}
