using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SmsService.Domain.Enums;

[JsonConverter(typeof(StringEnumConverter))]
public enum ConsentSource
{
    Checkout = 1,
    AccountSettings = 2,
    SupportRequest = 3
}
