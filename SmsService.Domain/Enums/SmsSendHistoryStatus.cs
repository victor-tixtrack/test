namespace SmsService.Domain.Enums;

public enum SmsSendHistoryStatus
{
    Sent = 1,
    Failed = 2,
    SkippedNoConsent = 3,
    BlockedOptedOut = 4,
}
