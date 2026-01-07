# TixTrack SMS Notification Service - Architecture Design

## Overview

A vendor-agnostic SMS notification service for sending order confirmation messages, designed to integrate with the existing TixTrack order flow while maintaining flexibility to swap SMS providers.

## Architecture Decisions

### Event-Driven Design
- **Trigger**: Thin event (`SendOrderConfirmationSms`) published to Azure Service Bus after order confirmation email is sent
- **Payload**: Minimal - just `OrderId` and `CustomerId`
- **Rationale**: Keeps coupling low while ensuring the SMS service retrieves current order state (avoids sending confirmations for cancelled orders)

### Data Access Strategy
- **Shared tables (read-only)**: Orders, Customers - accessed directly via the SMS service's data layer
- **Owned tables (read/write)**: `SmsConsent`, `SmsSendHistory` - managed exclusively by the SMS service
- **Rationale**: Pragmatic approach that avoids unnecessary API hops while maintaining clear domain ownership

### Provider Abstraction
- **Pattern**: Port/Adapter (Hexagonal Architecture)
- **Interface**: `ISmsProvider` with initial `TwilioSmsProvider` implementation
- **Rationale**: Enables swapping providers without modifying business logic

### Multi-Tenancy: Dedicated Phone Numbers per Venue
- **Strategy**: Each venue gets its own dedicated SMS provider phone number
- **Consumer Experience**: Ticket buyers always see the same sender number for each venue they interact with
- **Isolation**: Natural separation between venues - consumer buying tickets at multiple venues receives messages from different numbers
- **Compliance**: Each venue manages opt-outs independently (consumer can opt out of one venue but still receive from others)
- **Rationale**:
  - Better brand identity and trust (consistent sender per venue)
  - Simpler deliverability management (each venue has distinct sender reputation)
  - Phone number costs are negligible compared to B2B contract values
  - Cleaner compliance model (opt-outs are venue-specific, not customer-wide)

### Consent Management
- **SMS service owns all consent data** - not derived from user account flags
- **Consent scope**: Per-venue (consumer can opt out of one venue but remain opted in for others)
- **Initial consent**: Captured when user provides phone number during checkout (checkbox)
- **Opt-out handling**: Provider webhook updates local consent store for specific venue
- **Rationale**: Single source of truth for SMS channel; carrier-level opt-outs are captured via webhook; venue-scoped consent provides better consumer control

## Components

### SMS Notification Service (Azure Container Apps)

#### API Layer
| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/webhook/sms-provider` | POST | Receive STOP/START messages from SMS provider |
| `/api/consent/{venueId}/{phone}` | GET | Query consent status for venue (for other services) |
| `/api/history` | GET | Audit/compliance queries |

#### Service Layer
- **Service Bus Consumer**: Subscribes to `SendOrderConfirmationSms` topic
- **SMS Orchestrator**: Coordinates data retrieval, consent check, send, and history recording
- **Consent Service**: Manages opt-in/opt-out state
- **Template Engine**: Formats messages from templates + order data

#### Data Layer
- **Order Repository**: Read-only access to shared order/customer tables
- **Consent Repository**: CRUD for `SmsConsent` table
- **History Repository**: Write to `SmsSendHistory` table

### Database Tables (SMS-Owned)

```sql
-- Phone number assignments per venue
CREATE TABLE VenuePhoneNumbers (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    VenueId UNIQUEIDENTIFIER NOT NULL, -- FK to Venues table
    PhoneNumber NVARCHAR(20) NOT NULL, -- Provider phone number (E.164 format)
    ProviderId NVARCHAR(100) NOT NULL, -- Provider-specific identifier (e.g., Twilio Phone Number SID)
    ProviderName NVARCHAR(50) NOT NULL, -- 'twilio', 'plivo', etc.
    Status NVARCHAR(20) NOT NULL, -- 'active', 'inactive', 'released'
    AssignedAt DATETIME2 NOT NULL,
    ReleasedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NOT NULL,
    UNIQUE INDEX IX_VenuePhoneNumbers_Venue (VenueId) WHERE Status = 'active',
    INDEX IX_VenuePhoneNumbers_Phone (PhoneNumber)
);

-- Consent tracking (scoped to venue + consumer phone)
CREATE TABLE SmsConsent (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    VenueId UNIQUEIDENTIFIER NOT NULL, -- Consent is venue-specific
    PhoneNumber NVARCHAR(20) NOT NULL, -- Consumer phone number
    Status NVARCHAR(20) NOT NULL, -- 'opted_in', 'opted_out'
    InitialConsentSource NVARCHAR(50), -- 'checkout', 'account_settings'
    InitialConsentAt DATETIME2,
    OptedOutAt DATETIME2 NULL,
    OptedInAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NOT NULL,
    UNIQUE INDEX IX_SmsConsent_Venue_Phone (VenueId, PhoneNumber),
    INDEX IX_SmsConsent_Phone (PhoneNumber)
);

-- Send history for audit/compliance
CREATE TABLE SmsSendHistory (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    OrderId UNIQUEIDENTIFIER NOT NULL,
    VenueId UNIQUEIDENTIFIER NOT NULL,
    CustomerId UNIQUEIDENTIFIER NOT NULL,
    PhoneNumber NVARCHAR(20) NOT NULL, -- Consumer phone number
    SenderPhoneNumber NVARCHAR(20) NOT NULL, -- Venue's provider number used to send
    MessageType NVARCHAR(50) NOT NULL, -- 'order_confirmation'
    Status NVARCHAR(50) NOT NULL, -- 'sent', 'failed', 'skipped_no_consent', 'blocked_opted_out'
    ProviderMessageId NVARCHAR(100) NULL, -- Provider-specific message ID (e.g., Twilio Message SID)
    ProviderName NVARCHAR(50) NOT NULL, -- 'twilio', 'plivo', etc.
    ErrorCode NVARCHAR(20) NULL,
    ErrorMessage NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL,
    INDEX IX_SmsSendHistory_Order (OrderId),
    INDEX IX_SmsSendHistory_Venue (VenueId),
    INDEX IX_SmsSendHistory_Phone (PhoneNumber),
    INDEX IX_SmsSendHistory_Created (CreatedAt)
);
```

## Flows

### Order Confirmation SMS Flow
1. Payment succeeds → Order handler sends confirmation email
2. Order handler publishes `SendOrderConfirmationSms` event to Service Bus
3. SMS Service receives event
4. Queries database for order status and details (including VenueId)
5. If order is cancelled/invalid → log and skip
6. Queries customer data (phone, name)
7. Looks up venue's assigned phone number from `VenuePhoneNumbers` table
8. If no phone number assigned → provision new provider number, store in table
9. Checks `SmsConsent` table for (VenueId, consumer phone)
10. If not consented → log, record in history, skip
11. Formats message using template
12. Sends via `ISmsProvider` using venue's phone number as sender
13. Records result in `SmsSendHistory` (including VenueId and sender number)
14. Emits metrics to Datadog

### Opt-Out Webhook Flow
1. Consumer texts "STOP" to venue's provider number
2. Provider auto-replies and calls webhook with message details
3. SMS Service validates provider signature
4. Looks up VenueId from `VenuePhoneNumbers` based on recipient number
5. Updates `SmsConsent` to `opted_out` for (VenueId, consumer phone)
6. Logs event, emits metric
7. **Note**: Consumer can still receive SMS from other venues they've purchased from

### Error Handling
| Scenario | Action |
|----------|--------|
| Order cancelled | Skip, ack message |
| No consent | Skip, record in history |
| Provider 5xx error | Retry (nack message) |
| Provider opted-out error | Update consent, ack |
| Invalid phone error | Record failure, ack |

## Observability

### Datadog Metrics
- `sms.sent.total` - Counter by message_type, provider
- `sms.failed.total` - Counter by error_code, provider
- `sms.opt_out.total` - Counter
- `sms.opt_in.total` - Counter
- `sms.send.duration` - Histogram

### Application Insights
- Distributed tracing (correlation ID from Service Bus message)
- Structured logging for all operations
- Dependency tracking (database, SMS provider API)

### Alerts
- Send failure rate > 5%
- Webhook signature validation failures
- Provider API latency > 2s
- Dead letter queue depth > 0

## Future Extensibility

### Additional Message Types
Add new event types (e.g., `SendEventReminderSms`) and handlers without modifying core infrastructure.

### Additional Providers
Implement `ISmsProvider` for Plivo, MessageBird, etc. Provider selection can be configuration-driven.

### Multi-Region
Consent and history tables can be partitioned by region for UK/Australia expansion. Provider routing can be region-aware.
