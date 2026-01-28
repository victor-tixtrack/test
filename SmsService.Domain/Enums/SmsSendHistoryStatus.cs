using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SmsService.Domain.Enums;

[JsonConverter(typeof(StringEnumConverter))]
public enum SmsSendHistoryStatus
{
    Sent = 1,
    Failed = 2,
    SkippedNoConsent = 3,
    BlockedOptedOut = 4,
}
