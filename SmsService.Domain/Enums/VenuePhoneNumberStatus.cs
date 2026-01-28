using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SmsService.Domain.Enums;

[JsonConverter(typeof(StringEnumConverter))]
public enum VenuePhoneNumberStatus
{
    Active = 1,
    Inactive = 2,
    Released = 3,
}
