# SMS Notification Service - Jira Implementation Plan

## Epic: SMS Notification Service for Order Confirmations

**Epic Description:** Build a vendor-agnostic SMS notification service to send order confirmation messages via Twilio, with support for consent management and provider abstraction.

**Business Value:** Improve customer experience by providing timely SMS notifications for order confirmations, increasing engagement and reducing support inquiries.

---

## Sprint 1: Foundation & Database Setup

### Story 1.1: Project Setup and Solution Structure
**Story Points:** 3
**Priority:** Highest

**Description:**
Create the foundational .NET solution structure with clean architecture layers.

**Acceptance Criteria:**
- [ ] Solution created with 4 projects: `SmsService.Api`, `SmsService.Core`, `SmsService.Infrastructure`, `SmsService.Tests`
- [ ] Project references configured correctly
- [ ] Basic folder structure established (Models, Interfaces, Services, Repositories)
- [ ] .gitignore configured for .NET projects
- [ ] README.md with project overview and setup instructions

**Technical Notes:**
- Use .NET 8.0
- Follow clean architecture pattern
- Core should have no external dependencies
- Api references Infrastructure, Infrastructure references Core

---

### Story 1.2: Database Schema - SmsConsent Table
**Story Points:** 2
**Priority:** Highest

**Description:**
Create the database schema for SMS consent tracking.

**Acceptance Criteria:**
- [ ] SQL migration script created for `SmsConsent` table
- [ ] Table includes all columns from design doc (Id, PhoneNumber, Status, etc.)
- [ ] Index on PhoneNumber created
- [ ] Rollback script included
- [ ] Schema validated against requirements

**Technical Notes:**
- Use UNIQUEIDENTIFIER for Id
- Status values: 'opted_in', 'opted_out'
- Include audit timestamps (CreatedAt, UpdatedAt)

---

### Story 1.3: Database Schema - SmsSendHistory Table
**Story Points:** 2
**Priority:** Highest

**Description:**
Create the database schema for SMS send history tracking.

**Acceptance Criteria:**
- [ ] SQL migration script created for `SmsSendHistory` table
- [ ] Table includes all columns from design doc
- [ ] Indexes created on OrderId, PhoneNumber, and CreatedAt
- [ ] Rollback script included
- [ ] Schema validated against requirements

**Technical Notes:**
- Status values: 'sent', 'failed', 'skipped_no_consent', 'blocked_opted_out'
- Include provider tracking (ProviderName, ProviderMessageId)

---

## Sprint 2: Core Domain Layer

### Story 2.1: Core Domain Models and DTOs
**Story Points:** 3
**Priority:** High

**Description:**
Create all domain models, enums, and DTOs for the SMS service.

**Acceptance Criteria:**
- [ ] `SmsConsent` entity created
- [ ] `SmsSendHistory` entity created
- [ ] `SendOrderConfirmationSmsEvent` DTO created
- [ ] Enums created: `ConsentStatus`, `SendStatus`, `MessageType`
- [ ] Value objects for PhoneNumber (with validation)
- [ ] Unit tests for domain model validation

**Technical Notes:**
- Use records for immutable DTOs
- Add phone number format validation
- Include XML documentation

---

### Story 2.2: Core Interfaces - Repositories
**Story Points:** 2
**Priority:** High

**Description:**
Define repository interfaces for data access.

**Acceptance Criteria:**
- [ ] `IConsentRepository` interface created (CRUD operations)
- [ ] `IHistoryRepository` interface created (write + query)
- [ ] `IOrderRepository` interface created (read-only)
- [ ] Async method signatures defined
- [ ] XML documentation on all interfaces

**Technical Notes:**
- Follow repository pattern
- Return domain models, not EF entities
- Use CancellationToken in async methods

---

### Story 2.3: Core Interfaces - SMS Provider Abstraction
**Story Points:** 2
**Priority:** High

**Description:**
Define the `ISmsProvider` interface for vendor abstraction.

**Acceptance Criteria:**
- [ ] `ISmsProvider` interface created
- [ ] `SendSmsAsync` method defined with request/response models
- [ ] `SmsRequest` and `SmsResponse` DTOs created
- [ ] Provider-agnostic error handling model defined
- [ ] XML documentation

**Technical Notes:**
- Interface should be provider-agnostic (no Twilio-specific types)
- Include phone number, message body, callback URL in request
- Response should include success/failure and provider message ID

---

### Story 2.4: Consent Service Implementation
**Story Points:** 5
**Priority:** High

**Description:**
Implement the consent management service.

**Acceptance Criteria:**
- [ ] `ConsentService` class created implementing `IConsentService`
- [ ] `CheckConsentAsync` method validates opt-in status
- [ ] `RecordOptOutAsync` method updates consent to opted_out
- [ ] `RecordOptInAsync` method updates consent to opted_in
- [ ] `CreateInitialConsentAsync` method for checkout flow
- [ ] Unit tests with mocked repository (>80% coverage)
- [ ] Proper logging added

**Technical Notes:**
- Check consent status before allowing SMS send
- Update timestamps appropriately
- Handle edge cases (phone not found, duplicate opt-out)

---

### Story 2.5: Message Template Engine
**Story Points:** 3
**Priority:** High

**Description:**
Create a simple template engine for formatting SMS messages.

**Acceptance Criteria:**
- [ ] `MessageTemplateEngine` class created
- [ ] `FormatOrderConfirmationMessage` method implemented
- [ ] Template supports placeholders: {CustomerName}, {OrderNumber}, {EventName}, {EventDate}
- [ ] Message length validation (160 chars for single SMS)
- [ ] Unit tests for various scenarios
- [ ] Warning logged if message exceeds single SMS length

**Technical Notes:**
- Keep templates simple (string replacement)
- Future: consider moving templates to configuration/database
- Example: "Hi {CustomerName}, your order {OrderNumber} for {EventName} on {EventDate} is confirmed!"

---

## Sprint 3: Infrastructure Layer - Data Access

### Story 3.1: Entity Framework DbContext Setup
**Story Points:** 3
**Priority:** High

**Description:**
Set up Entity Framework Core with DbContext for SMS service tables.

**Acceptance Criteria:**
- [ ] `SmsDbContext` created with DbSets for SmsConsent and SmsSendHistory
- [ ] Entity configurations created (fluent API)
- [ ] Connection string configuration in appsettings
- [ ] Database migrations working
- [ ] Integration test verifying database connectivity

**Technical Notes:**
- Use SQL Server provider
- Configure entity mappings (column names, types, indexes)
- Enable query splitting for complex queries

---

### Story 3.2: Consent Repository Implementation
**Story Points:** 3
**Priority:** High

**Description:**
Implement the consent repository using Entity Framework.

**Acceptance Criteria:**
- [ ] `ConsentRepository` implements `IConsentRepository`
- [ ] All CRUD operations implemented
- [ ] `GetByPhoneNumberAsync` method implemented
- [ ] Proper error handling for database exceptions
- [ ] Integration tests with in-memory database

**Technical Notes:**
- Use async/await throughout
- Add retry policy for transient failures
- Log all database operations

---

### Story 3.3: History Repository Implementation
**Story Points:** 2
**Priority:** High

**Description:**
Implement the history repository for audit tracking.

**Acceptance Criteria:**
- [ ] `HistoryRepository` implements `IHistoryRepository`
- [ ] `CreateAsync` method for recording sends
- [ ] Query methods: `GetByOrderIdAsync`, `GetByPhoneNumberAsync`, `GetRecentAsync`
- [ ] Integration tests

**Technical Notes:**
- Write-heavy, optimize for inserts
- Consider batching for high volume (future)

---

### Story 3.4: Order Repository (Read-Only)
**Story Points:** 3
**Priority:** High

**Description:**
Create read-only repository for accessing shared order and customer data.

**Acceptance Criteria:**
- [ ] `OrderRepository` implements `IOrderRepository`
- [ ] `GetOrderWithCustomerAsync` method returns order + customer details
- [ ] Queries only fetch required fields (projection)
- [ ] Handle cancelled/invalid orders
- [ ] Integration tests with test data

**Technical Notes:**
- Access existing Orders and Customers tables (read-only)
- Use AsNoTracking() for read-only queries
- Return null if order not found or cancelled

---

## Sprint 4: Infrastructure Layer - Twilio Integration

### Story 4.1: Twilio SMS Provider Implementation
**Story Points:** 5
**Priority:** High

**Description:**
Implement the Twilio adapter for sending SMS messages.

**Acceptance Criteria:**
- [ ] `TwilioSmsProvider` implements `ISmsProvider`
- [ ] Twilio SDK integrated (Twilio NuGet package)
- [ ] `SendSmsAsync` calls Twilio API
- [ ] Configuration for Account SID, Auth Token, From Number
- [ ] Error mapping from Twilio error codes to domain errors
- [ ] Unit tests with mocked Twilio client
- [ ] Handle rate limiting and retries

**Technical Notes:**
- Map Twilio errors: 21610 (opted out), 21211 (invalid phone), 5xx (retry)
- Return provider message ID (SID) in response
- Set status callback URL for delivery receipts
- Add circuit breaker for Twilio API failures

---

### Story 4.2: Twilio Configuration and Secrets Management
**Story Points:** 2
**Priority:** High

**Description:**
Configure Twilio credentials and settings securely.

**Acceptance Criteria:**
- [ ] Twilio settings in appsettings.json (non-sensitive)
- [ ] Azure Key Vault integration for secrets (Account SID, Auth Token)
- [ ] Configuration class `TwilioOptions` created
- [ ] Validation on startup (fail fast if misconfigured)
- [ ] Documentation on required settings

**Technical Notes:**
- Never commit secrets to source control
- Use managed identity for Key Vault access in Azure
- Local development: use user secrets or local.settings.json

---

## Sprint 5: Service Bus Integration

### Story 5.1: Service Bus Message Consumer Setup
**Story Points:** 5
**Priority:** High

**Description:**
Set up Azure Service Bus consumer to receive `SendOrderConfirmationSms` events.

**Acceptance Criteria:**
- [ ] Service Bus connection configured
- [ ] Topic subscription created for `SendOrderConfirmationSms`
- [ ] Background service/hosted service created to process messages
- [ ] Message deserialization working
- [ ] Dead letter queue configured
- [ ] Integration test with local Service Bus emulator

**Technical Notes:**
- Use Azure.Messaging.ServiceBus SDK
- Set max concurrent calls (start with 10)
- Configure message lock duration (5 minutes)
- Enable auto-renewal of message lock

---

### Story 5.2: SMS Orchestrator Service
**Story Points:** 8
**Priority:** High

**Description:**
Implement the main orchestration logic for processing SMS send requests.

**Acceptance Criteria:**
- [ ] `SmsOrchestrator` class created
- [ ] `ProcessOrderConfirmationAsync` method implements full flow from design doc
- [ ] Order validation (check if cancelled)
- [ ] Consent check integration
- [ ] Message templating integration
- [ ] SMS provider integration
- [ ] History recording
- [ ] Proper error handling for all scenarios
- [ ] Unit tests with mocked dependencies (>80% coverage)
- [ ] Structured logging at each step

**Technical Notes:**
- This is the main business logic coordinator
- Follow the 12-step flow from the architecture doc
- Handle all error scenarios gracefully
- Complete message (ack) even on skipped sends

---

### Story 5.3: Service Bus Error Handling and Retry Logic
**Story Points:** 3
**Priority:** High

**Description:**
Implement retry and error handling for Service Bus message processing.

**Acceptance Criteria:**
- [ ] Retry policy configured for transient failures
- [ ] Dead letter queue handling for permanent failures
- [ ] Exponential backoff implemented
- [ ] Max retry count set (3)
- [ ] Logging for failed messages
- [ ] Alert/metric when DLQ depth > 0

**Technical Notes:**
- Retry on Twilio 5xx errors
- Don't retry on: no consent, invalid phone, opted out
- Log correlation ID for tracing

---

## Sprint 6: API Layer - Webhooks & Endpoints

### Story 6.1: Twilio Webhook Endpoint - Opt-Out Handling
**Story Points:** 5
**Priority:** High

**Description:**
Create webhook endpoint to handle STOP/START messages from Twilio.

**Acceptance Criteria:**
- [ ] `POST /webhook/twilio` endpoint created
- [ ] Twilio signature validation middleware implemented
- [ ] Parse incoming webhook payload (STOP/START)
- [ ] Call `ConsentService` to update opt-out status
- [ ] Return 200 OK to Twilio
- [ ] Unit tests with sample Twilio payloads
- [ ] Integration test

**Technical Notes:**
- Validate X-Twilio-Signature header
- Handle "STOP", "UNSTOP", "START" keywords
- Respond quickly (<5 seconds) to avoid Twilio retry
- Use async processing if needed

---

### Story 6.2: Consent Query API
**Story Points:** 2
**Priority:** Medium

**Description:**
Create API endpoint for querying consent status.

**Acceptance Criteria:**
- [ ] `GET /api/consent/{phone}` endpoint created
- [ ] Returns consent status for phone number
- [ ] Returns 404 if phone not found
- [ ] Phone number format validation
- [ ] API documentation (Swagger)
- [ ] Unit tests

**Technical Notes:**
- Used by other services to check consent before sending SMS
- Consider caching for frequently queried numbers
- Add rate limiting

---

### Story 6.3: SMS History Query API
**Story Points:** 3
**Priority:** Medium

**Description:**
Create API endpoint for querying SMS send history (audit/compliance).

**Acceptance Criteria:**
- [ ] `GET /api/history` endpoint created with query parameters
- [ ] Filter by: OrderId, PhoneNumber, DateRange, Status
- [ ] Pagination support (page, pageSize)
- [ ] Returns send history records
- [ ] API documentation (Swagger)
- [ ] Unit tests

**Technical Notes:**
- Add authorization (internal use only)
- Consider performance for large datasets
- Add indexes on queried columns

---

### Story 6.4: Health Check Endpoints
**Story Points:** 2
**Priority:** Medium

**Description:**
Add health check endpoints for monitoring.

**Acceptance Criteria:**
- [ ] `/health` endpoint returns service health
- [ ] Database connectivity check
- [ ] Service Bus connectivity check
- [ ] Twilio API connectivity check (optional)
- [ ] Returns 200 if healthy, 503 if unhealthy
- [ ] Integration test

**Technical Notes:**
- Use ASP.NET Core health checks
- Don't call external APIs too frequently
- Used by Azure Container Apps for liveness/readiness probes

---

## Sprint 7: Observability & Monitoring

### Story 7.1: Structured Logging with Application Insights
**Story Points:** 3
**Priority:** High

**Description:**
Implement comprehensive structured logging throughout the application.

**Acceptance Criteria:**
- [ ] Application Insights SDK integrated
- [ ] Structured logging added to all services
- [ ] Correlation IDs propagated from Service Bus messages
- [ ] Log levels configured appropriately
- [ ] Exception tracking configured
- [ ] Dependency tracking enabled (database, Twilio)

**Technical Notes:**
- Use ILogger throughout
- Log at appropriate levels (Info, Warning, Error)
- Include relevant context (OrderId, PhoneNumber, etc.)
- Avoid logging PII in production

---

### Story 7.2: Datadog Metrics Integration
**Story Points:** 5
**Priority:** High

**Description:**
Integrate Datadog for custom metrics tracking.

**Acceptance Criteria:**
- [ ] Datadog SDK integrated
- [ ] Metrics implemented from design doc:
  - `sms.sent.total` (counter by message_type, provider)
  - `sms.failed.total` (counter by error_code, provider)
  - `sms.opt_out.total` (counter)
  - `sms.opt_in.total` (counter)
  - `sms.send.duration` (histogram)
- [ ] Metrics emitted at appropriate points in code
- [ ] Datadog dashboard created
- [ ] Documentation on metrics

**Technical Notes:**
- Use DogStatsD client
- Tag metrics with relevant dimensions
- Monitor metric cardinality

---

### Story 7.3: Alerting Configuration
**Story Points:** 3
**Priority:** Medium

**Description:**
Configure alerts for critical service issues.

**Acceptance Criteria:**
- [ ] Alert for send failure rate > 5%
- [ ] Alert for webhook signature validation failures
- [ ] Alert for Twilio API latency > 2s
- [ ] Alert for dead letter queue depth > 0
- [ ] Alert routing configured (PagerDuty/Slack)
- [ ] Alert runbooks created

**Technical Notes:**
- Use Datadog or Azure Monitor for alerts
- Set appropriate thresholds and time windows
- Include context in alert messages
- Define escalation paths

---

## Sprint 8: Testing & Deployment

### Story 8.1: Unit Test Suite Completion
**Story Points:** 5
**Priority:** High

**Description:**
Ensure comprehensive unit test coverage across all layers.

**Acceptance Criteria:**
- [ ] All services have unit tests (>80% coverage)
- [ ] All repositories have unit tests
- [ ] Controllers have unit tests
- [ ] Mock all external dependencies
- [ ] Tests run in CI/CD pipeline
- [ ] Coverage report generated

**Technical Notes:**
- Use xUnit and Moq
- Test happy path and edge cases
- Use test fixtures for common setup

---

### Story 8.2: Integration Test Suite
**Story Points:** 5
**Priority:** High

**Description:**
Create integration tests for end-to-end scenarios.

**Acceptance Criteria:**
- [ ] Integration tests for Service Bus message processing
- [ ] Integration tests for webhook endpoints
- [ ] Integration tests for database operations
- [ ] Use test containers for SQL Server
- [ ] Use in-memory Service Bus for testing
- [ ] Tests run in CI/CD pipeline

**Technical Notes:**
- Use WebApplicationFactory for API tests
- Use Testcontainers for database
- Clean up test data after each test

---

### Story 8.3: Docker Configuration
**Story Points:** 3
**Priority:** High

**Description:**
Create Dockerfile and docker-compose for local development and deployment.

**Acceptance Criteria:**
- [ ] Dockerfile created for SmsService.Api
- [ ] Multi-stage build configured (build + runtime)
- [ ] docker-compose.yml for local development (with SQL Server)
- [ ] Environment variable configuration
- [ ] Image builds successfully
- [ ] Container runs locally

**Technical Notes:**
- Use official .NET 8.0 images
- Optimize layer caching
- Include health check in container

---

### Story 8.4: Azure Container Apps Deployment Configuration
**Story Points:** 5
**Priority:** High

**Description:**
Configure deployment to Azure Container Apps with IaC.

**Acceptance Criteria:**
- [ ] Bicep/Terraform template for Container Apps
- [ ] Environment variables configured
- [ ] Managed identity for Key Vault access
- [ ] Container Apps ingress configured (HTTPS only)
- [ ] Scaling rules defined (CPU/memory)
- [ ] Deployment pipeline in Azure DevOps/GitHub Actions

**Technical Notes:**
- Use managed identity for Azure resources
- Configure min/max replicas
- Set resource limits (CPU, memory)
- Enable container logs to Log Analytics

---

### Story 8.5: CI/CD Pipeline Setup
**Story Points:** 5
**Priority:** High

**Description:**
Create CI/CD pipeline for automated build, test, and deployment.

**Acceptance Criteria:**
- [ ] Build pipeline runs on every commit
- [ ] Unit tests run in pipeline
- [ ] Integration tests run in pipeline
- [ ] Code coverage check (fail if <80%)
- [ ] Docker image built and pushed to ACR
- [ ] Deployment to dev environment automated
- [ ] Manual approval gate for production

**Technical Notes:**
- Use GitHub Actions or Azure DevOps
- Cache dependencies for faster builds
- Run tests in parallel
- Tag images with git commit SHA

---

## Sprint 9: Documentation & Handoff

### Story 9.1: API Documentation
**Story Points:** 2
**Priority:** Medium

**Description:**
Complete API documentation with Swagger/OpenAPI.

**Acceptance Criteria:**
- [ ] Swagger UI enabled
- [ ] All endpoints documented
- [ ] Request/response examples included
- [ ] Authentication documented
- [ ] Error response codes documented
- [ ] Hosted at `/swagger` endpoint

**Technical Notes:**
- Use XML comments for Swagger generation
- Include example values
- Document rate limits

---

### Story 9.2: Operational Runbook
**Story Points:** 3
**Priority:** Medium

**Description:**
Create operational runbook for support team.

**Acceptance Criteria:**
- [ ] Architecture overview documented
- [ ] Common troubleshooting scenarios documented
- [ ] How to check logs and metrics
- [ ] How to manually process failed messages
- [ ] How to handle Twilio outages
- [ ] Emergency contact information
- [ ] Link to all dashboards and alerts

**Technical Notes:**
- Keep in team wiki or docs repository
- Include screenshots of dashboards
- Update as issues arise

---

### Story 9.3: Developer Setup Guide
**Story Points:** 2
**Priority:** Medium

**Description:**
Create comprehensive developer setup guide.

**Acceptance Criteria:**
- [ ] Prerequisites documented (SDKs, tools)
- [ ] Local development setup steps
- [ ] How to run database migrations
- [ ] How to configure local secrets
- [ ] How to run tests
- [ ] How to use docker-compose for local testing
- [ ] Troubleshooting common issues

**Technical Notes:**
- Keep README.md updated
- Include VS Code/Rider setup tips
- Document environment variables

---

## Future Enhancements (Backlog)

### Enhancement: Delivery Status Tracking
**Story Points:** 5
**Description:** Add endpoint for Twilio to post delivery status updates and track message delivery states.

---

### Enhancement: Additional Message Types
**Story Points:** 8
**Description:** Support event reminders, ticket delivery notifications, etc. Add new event types and templates.

---

### Enhancement: Multi-Provider Support
**Story Points:** 13
**Description:** Implement additional SMS providers (Plivo, MessageBird) with automatic failover.

---

### Enhancement: Rate Limiting & Throttling
**Story Points:** 5
**Description:** Implement configurable rate limits to avoid hitting provider limits.

---

### Enhancement: Self-Service Consent Management UI
**Story Points:** 8
**Description:** Customer portal for managing SMS preferences.

---

### Enhancement: A/B Testing for Message Templates
**Story Points:** 8
**Description:** Support multiple templates with analytics to optimize engagement.

---

## Dependencies & Prerequisites

**Before Sprint 1:**
- [ ] Azure subscription and resource group provisioned
- [ ] Azure Service Bus namespace created with topic
- [ ] SQL Server database provisioned (Dev environment)
- [ ] Twilio account created with phone number
- [ ] Datadog account and API key obtained
- [ ] Azure Container Registry created
- [ ] Access granted to shared Orders/Customers database (read-only)

---

## Risk Register

| Risk | Impact | Mitigation |
|------|--------|------------|
| Twilio API changes | Medium | Pin SDK version, monitor changelog |
| Service Bus message throughput | High | Load test early, configure scaling |
| Shared database schema changes | Medium | Monitor changes, version repositories |
| PII/compliance concerns | High | Security review before production, GDPR compliance check |
| Message delivery delays | Low | Set clear SLA expectations with stakeholders |

---

## Success Metrics

- **Delivery Rate:** >95% of SMS sent successfully
- **Performance:** P95 send latency < 3 seconds
- **Reliability:** 99.9% uptime
- **Compliance:** 0 SMS sent to opted-out users
- **Cost:** <$0.01 per SMS sent (including Twilio + infrastructure)
