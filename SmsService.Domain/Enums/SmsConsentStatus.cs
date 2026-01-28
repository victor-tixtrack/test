using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SmsService.Domain.Enums;

[JsonConverter(typeof(StringEnumConverter))]
public enum SmsConsentStatus
{
    OptedIn = 1,
    OptedOut = 2,
}
