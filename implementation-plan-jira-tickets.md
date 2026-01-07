# SMS Notification Service - Jira Implementation Plan

## Epic: SMS Notification Service for Order Confirmations

**Epic Description:** Build a vendor-agnostic SMS notification service to send order confirmation messages via Twilio, with support for consent management and provider abstraction. Deploy multiple times daily using feature flags in the Order Confirmation service.

**Business Value:** Improve customer experience by providing timely SMS notifications for order confirmations, increasing engagement and reducing support inquiries. Enable rapid iteration with feature flags and infrastructure-as-code deployments.

---

## Sprint 1: Skeleton Service & Infrastructure (Parallel Tracks)

**Overview:** Service development and infrastructure setup happen in parallel. Story 1.1-1.2 can start immediately without waiting for Azure infrastructure. Stories 1.3-1.5 run in parallel.

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
- [ ] Dockerfile and docker-compose.yml included for local development
- [ ] GitHub repository initialized and pushed

**Technical Notes:**
- Use .NET 10
- Follow clean architecture pattern
- Core should have no external dependencies
- Api references Infrastructure, Infrastructure references Core
- Dockerfile: multi-stage build (build stage + runtime stage)
- docker-compose.yml should enable local dev without Azure

**Dependencies:** None - Start immediately

---

### Story 1.2: Minimal API with Health Check
**Story Points:** 5
**Priority:** Highest

**Description:**
Create a minimal API service that starts quickly with health check endpoints.

**Acceptance Criteria:**
- [ ] ASP.NET Core minimal API created
- [ ] `GET /health` endpoint returns `{ "status": "healthy" }` with 200
- [ ] `GET /health/live` liveness probe (instant response)
- [ ] `GET /health/ready` readiness probe (checks dependencies when configured)
- [ ] Service starts and responds in <5 seconds
- [ ] Structured logging configured (console output, Serilog)
- [ ] Dependency injection configured
- [ ] Runs locally with `docker-compose up`
- [ ] Can run without Azure infrastructure (local development ready)

**Technical Notes:**
- Health check should be lightweight (cache results for 10 seconds)
- Ready probe checks: database connection pool, Service Bus connectivity (when integrated)
- Live probe: always returns 200 (fail-fast only)
- Use Serilog for structured logging
- Appsettings.json for configuration (dev, staging, prod)
- No business logic yet, just infrastructure endpoints

**Dependencies:** Story 1.1 - Can start immediately after

---

### Story 1.3: GitHub Actions CI/CD Pipeline Setup
**Story Points:** 5
**Priority:** Highest

**Description:**
Set up GitHub Actions CI/CD pipeline for build, test, and deployment to Azure.

**Acceptance Criteria:**
- [ ] GitHub Actions workflow created (.github/workflows/)
- [ ] Build job: dotnet build on every push
- [ ] Unit test job: dotnet test with coverage reports
- [ ] Docker build: image tagged with commit SHA
- [ ] Push to Azure Container Registry (ACR)
- [ ] Deploy to dev environment (manual approval gate for prod)
- [ ] Workflow runs in parallel for speed
- [ ] Coverage threshold enforced (fail if <80%)

**Technical Notes:**
- Use dotnet 10 SDK image
- Cache NuGet packages between runs
- Tag images: `acr.azurecr.io/nliven-sms:latest` and `acr.azurecr.io/nliven-sms:${{ github.sha }}`
- Dev environment deploys automatically on main branch
- Production requires manual approval
- Can run in parallel with Story 1.2

**Dependencies:** Story 1.1 - Can start immediately after

---

### Story 1.4: Terraform Infrastructure - Base Resources
**Story Points:** 8
**Priority:** Highest

**Description:**
Create Terraform configuration for core Azure resources.

**Acceptance Criteria:**
- [ ] Terraform project structure created (main.tf, variables.tf, outputs.tf)
- [ ] Resource group variables configured
- [ ] Azure Container Registry (ACR) resource
- [ ] Azure Container Apps environment
- [ ] Key Vault for secrets management
- [ ] Log Analytics workspace
- [ ] Variables for dev/prod environments
- [ ] Terraform state stored in Azure Storage backend

**Technical Notes:**
- Use modules for reusability
- Separate dev and prod tfvars files
- Output: ACR URL, Container Apps domain, Key Vault URI
- Include data sources for existing VNets if applicable
- Local development: use Terraform Cloud or Azure Storage for state
- Can be done in parallel with Story 1.2 (service development)

**Dependencies:** None - Can start immediately in parallel with Story 1.2

---

### Story 1.5: Terraform Infrastructure - Container Apps Deployment
**Story Points:** 5
**Priority:** Highest

**Description:**
Create Terraform configuration to deploy service to Azure Container Apps.

**Acceptance Criteria:**
- [ ] Container Apps resource configured
- [ ] Image from ACR integrated
- [ ] Environment variables for dev/prod
- [ ] Managed identity for Key Vault access
- [ ] Liveness and readiness probes configured (health check endpoint)
- [ ] Ingress configured (HTTPS only)
- [ ] Min replicas: 1, Max replicas: 3
- [ ] CPU: 0.5, Memory: 1GB
- [ ] Application Insights integration

**Technical Notes:**
- Health check endpoint: `GET /health` (returns 200 if healthy)
- Liveness probe: /health, 30s interval, 10s timeout
- Readiness probe: /health, 5s interval
- Scale rule: CPU > 70% triggers scale out
- Manual scale down to 0 replicas not allowed for reliability

**Dependencies:** Story 1.4 (Terraform base), Story 1.3 (GitHub Actions) - Merges both tracks

---

## Sprint 2: Database Schema & Domain Models

### Story 2.1: Database DDL - VenuePhoneNumbers Table
**Story Points:** 2
**Priority:** Highest

**Description:**
Create the database schema for venue phone number assignments.

**Acceptance Criteria:**
- [ ] SQL migration script created for `VenuePhoneNumbers` table
- [ ] Columns: Id (PK), VenueId (FK), PhoneNumber, ProviderName, CreatedAt, UpdatedAt
- [ ] Unique constraint on VenueId + ProviderName
- [ ] Index on VenueId
- [ ] Rollback script included
- [ ] Migration file added to project

**Technical Notes:**
- Use UNIQUEIDENTIFIER for Id
- Migration tool: Entity Framework Core Migrations
- Store in `/src/SmsService.Infrastructure/Migrations/` folder
- This is DDL-only, no EF entity mapping yet

---

### Story 2.2: Database DDL - SmsConsent Table
**Story Points:** 2
**Priority:** Highest

**Description:**
Create the database schema for SMS consent tracking.

**Acceptance Criteria:**
- [ ] SQL migration script created for `SmsConsent` table
- [ ] Columns: Id (PK), VenueId, PhoneNumber, ConsentStatus, CreatedAt, UpdatedAt
- [ ] Unique constraint on VenueId + PhoneNumber
- [ ] Index on PhoneNumber
- [ ] Index on VenueId
- [ ] Rollback script included
- [ ] Migration file added to project

**Technical Notes:**
- Status values: 'opted_in', 'opted_out'
- ConsentStatus column type: VARCHAR(20)
- Separate DDL from entity mapping (Story 2.5)

---

### Story 2.3: Database DDL - SmsSendHistory Table
**Story Points:** 2
**Priority:** Highest

**Description:**
Create the database schema for SMS send history tracking.

**Acceptance Criteria:**
- [ ] SQL migration script created for `SmsSendHistory` table
- [ ] Columns: Id (PK), OrderId, VenueId, PhoneNumber, Status, Message, ProviderName, ProviderMessageId, ErrorCode, CreatedAt
- [ ] Indexes on: OrderId, PhoneNumber, VenueId, CreatedAt
- [ ] Rollback script included
- [ ] Migration file added to project

**Technical Notes:**
- Status values: 'sent', 'failed', 'skipped_no_consent', 'blocked_opted_out'
- Status column type: VARCHAR(30)
- Large table expected, optimize indexes for queries
- Separate DDL from entity mapping (Story 2.4)

---

### Story 2.4: Core Domain Models and Value Objects
**Story Points:** 3
**Priority:** High

**Description:**
Create all domain models and value objects without EF entity configurations.

**Acceptance Criteria:**
- [ ] `SmsConsent` domain model created (record or class)
- [ ] `SmsSendHistory` domain model created
- [ ] `VenuePhoneNumber` domain model created
- [ ] `SendOrderConfirmationSmsEvent` DTO created
- [ ] Enums: `ConsentStatus`, `SendStatus`, `ProviderName`
- [ ] Value objects: `PhoneNumber` with validation
- [ ] All models have XML documentation
- [ ] Unit tests for PhoneNumber validation

**Technical Notes:**
- Use records for immutable DTOs (SendOrderConfirmationSmsEvent)
- Use classes for aggregate roots (SmsConsent, SmsSendHistory)
- PhoneNumber value object: validate format, length
- Models should NOT be EF-mapped yet (separation of concerns)
- Models live in SmsService.Core project

---

### Story 2.5: Entity Framework Entity Configurations
**Story Points:** 3
**Priority:** High

**Description:**
Create EF Core entity configurations mapping domain models to database schema.

**Acceptance Criteria:**
- [ ] DbContext created: `SmsDbContext`
- [ ] Entity configuration classes created (FluentAPI)
- [ ] Configurations for: SmsConsent, SmsSendHistory, VenuePhoneNumber
- [ ] Shadow properties for audit timestamps if needed
- [ ] Database indexes configured (mirroring DDL)
- [ ] Relationships configured
- [ ] DbContext configured in DI container

**Technical Notes:**
- Use IEntityTypeConfiguration<T> pattern
- Separate configuration classes in Infrastructure/Data/Configurations/
- Enable query tracking behavior (ReadOnly for queries, SaveChanges for writes)
- Connection string from appsettings.json

---

### Story 2.6: Core Interfaces - Repositories
**Story Points:** 2
**Priority:** High

**Description:**
Define repository interfaces for data access.

**Acceptance Criteria:**
- [ ] `IConsentRepository` interface created (CRUD for consent)
- [ ] `IHistoryRepository` interface created (write + query)
- [ ] `IOrderRepository` interface created (read-only, external)
- [ ] All methods are async (CancellationToken support)
- [ ] XML documentation on all methods
- [ ] Interfaces in Core project

**Technical Notes:**
- Repository pattern for domain models, not EF entities
- IOrderRepository queries external shared database
- Avoid N+1 queries (think about projection)
- Return domain models, not EF entities

---

### Story 2.7: Core Interfaces - SMS Provider Abstraction
**Story Points:** 2
**Priority:** High

**Description:**
Define the ISmsProvider interface for vendor abstraction.

**Acceptance Criteria:**
- [ ] `ISmsProvider` interface created
- [ ] `SendSmsAsync(request, cancellationToken)` method
- [ ] `SmsRequest` DTO (PhoneNumber, Message, CallbackUrl, VenueId)
- [ ] `SmsResponse` DTO (Success, MessageId, ErrorCode, ErrorMessage)
- [ ] Provider-agnostic error codes (enum)
- [ ] XML documentation

**Technical Notes:**
- Interface in Core project
- No Twilio-specific types
- Error codes: Success, InvalidPhone, OptedOut, RateLimited, ProviderError
- Request/Response in Core/Models/Sms/

---

## Sprint 3: Core Services

### Story 3.1: Consent Service Implementation
**Story Points:** 5
**Priority:** High

**Description:**
Implement the consent management service.

**Acceptance Criteria:**
- [ ] `ConsentService` class created in Infrastructure
- [ ] `CheckConsentAsync(venueId, phoneNumber)` - returns ConsentStatus
- [ ] `RecordOptOutAsync(venueId, phoneNumber)` - updates to opted_out
- [ ] `RecordOptInAsync(venueId, phoneNumber)` - updates to opted_in
- [ ] `CreateInitialConsentAsync(venueId, phoneNumber)` - creates opted_in
- [ ] Proper error handling (phone not found, validation)
- [ ] Timestamps updated on write operations
- [ ] Unit tests with mocked repository (>80% coverage)
- [ ] Logging added

**Technical Notes:**
- Injected via DI
- Handle race conditions (duplicate opt-out)
- Log all state changes
- Consider caching consent checks (Redis future enhancement)

---

### Story 3.2: Message Template Engine
**Story Points:** 3
**Priority:** High

**Description:**
Create a simple template engine for formatting SMS messages.

**Acceptance Criteria:**
- [ ] `MessageTemplateEngine` class created
- [ ] `FormatOrderConfirmationMessage(order, customer)` method
- [ ] Template placeholders: {CustomerName}, {OrderNumber}, {EventName}, {EventDate}
- [ ] Message length validation (warn if >160 chars)
- [ ] Unit tests for formatting scenarios
- [ ] Template stored in appsettings or constants

**Technical Notes:**
- Keep templates simple (string.Format or similar)
- Future: move templates to database
- Example: "Hi {CustomerName}, your order {OrderNumber} for {EventName} on {EventDate} is confirmed!"
- Log warning if SMS exceeds single segment length

---

### Story 3.3: SMS Orchestrator Service
**Story Points:** 8
**Priority:** High

**Description:**
Implement the main orchestration logic for processing SMS send requests.

**Acceptance Criteria:**
- [ ] `SmsOrchestrator` class created in Core/Services
- [ ] `ProcessOrderConfirmationAsync(event)` implements full flow
- [ ] Order validation (fetch order, check if cancelled)
- [ ] Consent check integration
- [ ] Message formatting
- [ ] SMS provider call with error handling
- [ ] History recording
- [ ] All error scenarios handled gracefully
- [ ] Unit tests with mocked dependencies (>80% coverage)
- [ ] Detailed logging at each step

**Technical Notes:**
- This is the domain service orchestrator
- Follow design doc flow (check consent before publishing event!)
- Handle all error codes from provider
- Always complete the message (record even skipped sends)
- Log correlation ID for tracing

---

---

## Sprint 4: Infrastructure Layer - Data Access

### Story 4.1: Consent Repository Implementation
**Story Points:** 3
**Priority:** High

**Description:**
Implement the consent repository using Entity Framework.

**Acceptance Criteria:**
- [ ] `ConsentRepository` implements `IConsentRepository`
- [ ] `GetByPhoneNumberAsync(venueId, phoneNumber)`
- [ ] `CreateAsync(consent)`
- [ ] `UpdateAsync(consent)`
- [ ] `DeleteAsync(id)`
- [ ] Proper error handling for database exceptions
- [ ] Integration tests with real database (Testcontainers)
- [ ] Async/await throughout

**Technical Notes:**
- Use async database calls
- Add retry logic for transient failures (Polly)
- Map domain model to EF entity
- Enable query tracking appropriately

---

### Story 4.2: SmsSendHistory Repository Implementation
**Story Points:** 2
**Priority:** High

**Description:**
Implement the history repository for audit tracking.

**Acceptance Criteria:**
- [ ] `SmsSendHistoryRepository` implements `IHistoryRepository`
- [ ] `CreateAsync(history)` - write send record
- [ ] `GetByOrderIdAsync(orderId)` - query by order
- [ ] `GetByPhoneNumberAsync(phoneNumber)` - query by phone
- [ ] `GetByVenueIdAsync(venueId, dateRange)` - compliance queries
- [ ] Integration tests
- [ ] Paging support for large result sets

**Technical Notes:**
- Write-heavy, optimize for inserts
- Return domain models, map from EF entities
- Consider temporal queries (future enhancement)

---

### Story 4.3: Order Repository (Read-Only External)
**Story Points:** 3
**Priority:** High

**Description:**
Create read-only repository for accessing shared order and customer data.

**Acceptance Criteria:**
- [ ] Separate DbContext for external Orders database
- [ ] `OrderRepository` implements `IOrderRepository`
- [ ] `GetOrderWithCustomerAsync(orderId)` - fetch order + customer
- [ ] Projection: fetch only needed fields (OrderNumber, CustomerName, etc.)
- [ ] Handle missing/cancelled orders (return null)
- [ ] Use AsNoTracking() for read-only
- [ ] Integration tests with test data

**Technical Notes:**
- Separate DbContext: `OrdersDbContext` (read-only)
- Connection string from appsettings (Orders.ConnectionString)
- No write operations in this repository
- Consider caching for high-volume reads (future)

---

## Sprint 5: SMS Provider Integration

### Story 5.1: Twilio SMS Provider Implementation
**Story Points:** 5
**Priority:** High

**Description:**
Implement the Twilio adapter for sending SMS messages.

**Acceptance Criteria:**
- [ ] `TwilioSmsProvider` implements `ISmsProvider`
- [ ] Twilio NuGet package integrated
- [ ] `SendSmsAsync` calls Twilio API
- [ ] Request mapping (phone, message, callback URL)
- [ ] Response mapping (success, message ID, errors)
- [ ] Error mapping: 21610 (opted out), 21211 (invalid), 5xx (retry)
- [ ] Unit tests with mocked Twilio client
- [ ] Timeout handling (30 second timeout)
- [ ] Logging for all calls

**Technical Notes:**
- Use Twilio.Sdk NuGet package
- Map domain error codes to Twilio error codes
- Set status callback URL for delivery receipts
- Add circuit breaker pattern (Polly) for resilience

---

### Story 5.2: Twilio Configuration and Secrets Management
**Story Points:** 2
**Priority:** High

**Description:**
Configure Twilio credentials securely using Key Vault.

**Acceptance Criteria:**
- [ ] TwilioOptions configuration class created
- [ ] appsettings.json (non-sensitive: AccountSidKey, AuthTokenKey)
- [ ] Azure Key Vault integration
- [ ] Managed identity for Key Vault access
- [ ] Configuration validation on startup (fail fast)
- [ ] Local development: User Secrets support
- [ ] Documentation for setting up secrets

**Technical Notes:**
- Keys in Key Vault: twilio-account-sid, twilio-auth-token, twilio-from-number
- Local dev: `dotnet user-secrets set "Twilio:AccountSid" "..."`
- Production: Terraform manages Key Vault secrets
- Never commit actual credentials

---

## Sprint 6: Service Bus Integration

### Story 6.1: Service Bus Message Consumer Setup
**Story Points:** 5
**Priority:** High

**Description:**
Set up Azure Service Bus consumer to receive order confirmation SMS events.

**Acceptance Criteria:**
- [ ] Service Bus connection configured from appsettings
- [ ] Topic subscription created for `SendOrderConfirmationSms`
- [ ] Background service: `SendOrderConfirmationSmsConsumer` (HostedService)
- [ ] Message deserialization (JSON to domain event)
- [ ] Dead letter queue configured (auto-dead-letter after 3 retries)
- [ ] Correlation ID propagation
- [ ] Integration test with Azurite or Service Bus emulator
- [ ] Graceful shutdown on app shutdown

**Technical Notes:**
- Use Azure.Messaging.ServiceBus SDK
- Max concurrent calls: 10
- Message lock duration: 5 minutes
- Auto-lock renewal enabled
- DLQ subscription created in Terraform

---

### Story 6.2: Service Bus Error Handling and Retry Logic
**Story Points:** 3
**Priority:** High

**Description:**
Implement retry and error handling for Service Bus message processing.

**Acceptance Criteria:**
- [ ] Retry policy configured (exponential backoff)
- [ ] Max retries: 3
- [ ] Retry delays: 1s, 2s, 4s
- [ ] Dead letter queue handling for permanent failures
- [ ] Logging for retries and DLQ messages
- [ ] Metrics: retry attempts, DLQ depth
- [ ] Alert when DLQ has messages

**Technical Notes:**
- Transient failures: Twilio 5xx, temporary database errors
- Non-transient (nack immediately): no consent, opted out, invalid phone
- Log correlation ID for tracing across retries

---

## Sprint 7: API Layer - Webhooks & Endpoints

### Story 7.1: Twilio Webhook Endpoint - Opt-Out Handling
**Story Points:** 5
**Priority:** High

**Description:**
Create webhook endpoint to handle STOP/START messages from Twilio.

**Acceptance Criteria:**
- [ ] `POST /webhook/twilio` endpoint created
- [ ] Twilio request signature validation middleware
- [ ] X-Twilio-Signature header validation (HMAC-SHA1)
- [ ] Parse webhook payload (extract STOP/START keywords)
- [ ] Call ConsentService.RecordOptOutAsync or RecordOptInAsync
- [ ] Return 200 OK to Twilio immediately
- [ ] Unit tests with sample Twilio payloads
- [ ] Integration test

**Technical Notes:**
- Signature validation using Twilio SDK
- Handle: STOP, START, UNSTOP keywords
- Respond within 5 seconds (async processing if needed)
- Log all webhook events
- Use correlation ID for tracing

---

### Story 7.2: Health Check Endpoints
**Story Points:** 2
**Priority:** High

**Description:**
Create health check endpoints for orchestration and monitoring.

**Acceptance Criteria:**
- [ ] `GET /health` - detailed health check (JSON response)
- [ ] `GET /health/live` - liveness probe (instant response)
- [ ] `GET /health/ready` - readiness probe (checks dependencies)
- [ ] Liveness: always returns 200 (fail-fast only)
- [ ] Readiness: checks database, Service Bus, Key Vault connectivity
- [ ] Cache readiness check results (10 second TTL)
- [ ] Returns 200 if ready, 503 if not
- [ ] Unit tests

**Technical Notes:**
- Use ASP.NET Core health checks API
- Ready checks should be lightweight (cache results)
- Used by Container Apps liveness/readiness probes
- Document in Swagger

---

### Story 7.3: Consent Query API (Future - Phase 2)
**Story Points:** 2
**Priority:** Medium

**Description:**
Create API endpoint for other services to query consent status.

**Acceptance Criteria:**
- [ ] `GET /api/consent/{venueId}/{phoneNumber}` endpoint
- [ ] Returns: { "consentStatus": "opted_in|opted_out" }
- [ ] Returns 404 if phone not found for venue
- [ ] Phone number format validation
- [ ] API documentation (Swagger)
- [ ] Unit tests

**Technical Notes:**
- Used by Order Confirmation service to check before creating event
- Consider caching with Redis (future)
- Add rate limiting

---

### Story 7.4: SMS History Query API (Future - Phase 2)
**Story Points:** 3
**Priority:** Medium

**Description:**
Create API endpoint for querying SMS send history (audit/compliance).

**Acceptance Criteria:**
- [ ] `GET /api/history` endpoint with query parameters
- [ ] Filter by: OrderId, PhoneNumber, VenueId, DateRange, Status
- [ ] Pagination support (page, pageSize)
- [ ] Returns send history records with metadata
- [ ] Authorization check (internal only)
- [ ] API documentation (Swagger)
- [ ] Unit tests

**Technical Notes:**
- Compliance queries (for support team)
- Consider performance for large datasets
- Add export to CSV (future)

---

## Sprint 8: Feature Flag Integration

### Story 8.1: Feature Flag Service Setup
**Story Points:** 3
**Priority:** High

**Description:**
Integrate feature flag service to control SMS flow from Order Confirmation service.

**Acceptance Criteria:**
- [ ] Feature flag provider selected (LaunchDarkly, Azure App Configuration, or custom)
- [ ] Configuration in SmsService for storing flag definitions
- [ ] `IFeatureFlagService` interface created
- [ ] `IsOrderConfirmationSmsEnabledAsync(venueId)` method
- [ ] Flags cached locally (TTL: 5 minutes)
- [ ] Unit tests for flag resolution
- [ ] Documentation for enabling/disabling flags per environment

**Technical Notes:**
- Supports gradual rollout by venue
- Order Confirmation service checks flag before publishing event to Service Bus
- Flag changes propagate within 5 minutes
- Metrics: when SMS flow is enabled/disabled

---

### Story 8.2: Order Confirmation Service Integration (External Story)
**Story Points:** 5
**Priority:** High

**Description:**
Integrate feature flag check in Order Confirmation service.

**Acceptance Criteria:**
- [ ] Dependency on Feature Flag service added
- [ ] Before publishing `SendOrderConfirmationSms` event, check feature flag
- [ ] If disabled: log and skip SMS event publishing (no error)
- [ ] If enabled: publish event as normal
- [ ] Unit tests with flag enabled/disabled scenarios
- [ ] Metrics: SMS flow enabled/disabled events

**Technical Notes:**
- No changes to Order Confirmation domain logic
- Feature flag check is non-blocking (fail-open)
- Log when SMS flow changes state
- This is in a separate repository, coordination required

---

## Sprint 9: Docker & Local Development

### Story 9.1: Dockerfile and Docker Compose
**Story Points:** 3
**Priority:** High

**Description:**
Create Dockerfile and docker-compose for development and deployment.

**Acceptance Criteria:**
- [ ] Dockerfile created with multi-stage build
- [ ] Build stage: restore, build, test
- [ ] Runtime stage: minimal runtime image
- [ ] Health check included in Dockerfile
- [ ] docker-compose.yml for local development
- [ ] Includes: SmsService, SQL Server, Service Bus emulator (Azurite)
- [ ] Environment variables for local development
- [ ] Volumes for code hot-reload
- [ ] Docker image builds successfully and runs locally

**Technical Notes:**
- Base image: mcr.microsoft.com/dotnet/aspnet:10
- Build image: mcr.microsoft.com/dotnet/sdk:10
- Health check: HEALTHCHECK CMD curl -f http://localhost:80/health || exit 1
- SQL Server image: mcr.microsoft.com/mssql/server:latest
- Service Bus emulator: Azure Storage emulator (Azurite)

---

### Story 9.2: Development Setup Guide
**Story Points:** 2
**Priority:** High

**Description:**
Create comprehensive developer setup guide.

**Acceptance Criteria:**
- [ ] README.md with architecture overview
- [ ] Prerequisites: .NET 10 SDK, Docker, Git
- [ ] Local development setup steps
- [ ] Run with docker-compose (one command)
- [ ] Run tests locally
- [ ] Debugging setup (VS Code, Rider)
- [ ] How to manage secrets locally (user-secrets)
- [ ] Troubleshooting common issues
- [ ] Database migration steps

**Technical Notes:**
- Keep updated as project evolves
- Include screenshots of Swagger UI, logs
- Document default credentials for local dev
- Include VS Code extensions recommendations

---

## Sprint 10: Testing & Observability

### Story 10.1: Unit Test Suite
**Story Points:** 5
**Priority:** High

**Description:**
Ensure comprehensive unit test coverage across all layers.

**Acceptance Criteria:**
- [ ] All domain services have unit tests (>80% coverage)
- [ ] All repositories have unit tests
- [ ] Controllers have unit tests
- [ ] Mock all external dependencies (ISmsProvider, IConsentRepository)
- [ ] xUnit test framework
- [ ] Moq for mocking
- [ ] Tests run in CI/CD pipeline
- [ ] Coverage report generated (OpenCover)

**Technical Notes:**
- Test happy path and edge cases
- Use test fixtures for common setup
- Test error scenarios (provider errors, validation)
- Fast tests (<1ms per test)

---

### Story 10.2: Integration Test Suite
**Story Points:** 5
**Priority:** High

**Description:**
Create integration tests for end-to-end scenarios.

**Acceptance Criteria:**
- [ ] Service Bus message processing integration tests
- [ ] Webhook endpoint integration tests
- [ ] Database repository integration tests
- [ ] Use Testcontainers for SQL Server (realistic)
- [ ] Use in-memory Service Bus for testing
- [ ] Use WebApplicationFactory for API tests
- [ ] Tests clean up data after execution
- [ ] Tests run in CI/CD pipeline (separate from unit tests)

**Technical Notes:**
- Testcontainers: testcontainers/testcontainers-dotnet
- Real database interactions (not mocked)
- Slow tests acceptable (<5 seconds per test)
- Test failed scenarios and error handling

---

### Story 10.3: Structured Logging with Application Insights
**Story Points:** 3
**Priority:** High

**Description:**
Implement comprehensive structured logging throughout the application.

**Acceptance Criteria:**
- [ ] Application Insights SDK integrated
- [ ] Serilog configured for structured logging
- [ ] Correlation IDs propagated from Service Bus messages
- [ ] All services log relevant context (OrderId, PhoneNumber, VenueId)
- [ ] Log levels: Info (normal flow), Warning (retries), Error (failures)
- [ ] Exception logging with stack traces
- [ ] PII protection (avoid logging phone numbers, email in production)

**Technical Notes:**
- Use ILogger<T> throughout
- Correlation ID in all log entries
- Azure Monitor integration (Application Insights)
- Test log output in unit tests

---

### Story 10.4: Datadog Metrics Integration (Optional)
**Story Points:** 5
**Priority:** Medium

**Description:**
Integrate Datadog for custom metrics tracking.

**Acceptance Criteria:**
- [ ] Datadog SDK integrated
- [ ] Metrics emitted:
  - `sms.sent.total` (counter)
  - `sms.failed.total` (counter)
  - `sms.opt_out.total` (counter)
  - `sms.send.duration` (histogram)
- [ ] Metrics tagged: venue_id, provider, error_code
- [ ] Datadog dashboard created
- [ ] Alerts configured for critical metrics

**Technical Notes:**
- Use Datadog.Trace NuGet package
- Emit metrics at service boundaries
- Keep cardinality low (avoid unbounded tags)

---

## Sprint 11: Deployment & Production Readiness

### Story 11.1: Pre-Production Deployment & Testing
**Story Points:** 5
**Priority:** High

**Description:**
Deploy to staging and validate end-to-end functionality.

**Acceptance Criteria:**
- [ ] Service deployed to staging via Terraform/GitHub Actions
- [ ] Health checks passing
- [ ] Manual smoke tests: send test SMS via Twilio
- [ ] Webhook test: simulate STOP message from Twilio
- [ ] Database migrations verified
- [ ] Logs flowing to Application Insights
- [ ] Metrics flowing to Datadog (if enabled)
- [ ] Performance tested: send 1000 messages, measure latency
- [ ] Load test: simulate realistic volume, verify scaling

**Technical Notes:**
- Use staging Twilio account with test number
- Monitor logs for any errors
- Verify telemetry pipeline end-to-end
- Document any issues found

---

### Story 11.2: Production Deployment Runbook
**Story Points:** 3
**Priority:** High

**Description:**
Create operational runbook for deploying to production.

**Acceptance Criteria:**
- [ ] Deployment steps documented
- [ ] Rollback procedure documented
- [ ] Manual approval gate in GitHub Actions
- [ ] Pre-deployment checklist (tests pass, coverage >80%)
- [ ] Post-deployment validation steps
- [ ] Emergency contact information
- [ ] Link to dashboards and alerts
- [ ] Common troubleshooting scenarios

**Technical Notes:**
- Deploy during low-traffic periods initially
- Use gradual rollout (feature flag)
- Monitor metrics closely for 30 minutes post-deploy
- Have rollback plan ready

---

### Story 11.3: Monitoring & Alerting Configuration
**Story Points:** 3
**Priority:** High

**Description:**
Configure alerts for critical service issues.

**Acceptance Criteria:**
- [ ] Alert: send failure rate > 5% (5-minute window)
- [ ] Alert: webhook signature validation failures > 0
- [ ] Alert: Service Bus DLQ depth > 0
- [ ] Alert: API latency p95 > 3 seconds
- [ ] Alert: database connection pool exhaustion
- [ ] Alert routing: Slack channel or PagerDuty
- [ ] Alert runbooks: what to do when alert fires
- [ ] Dashboard: real-time SMS metrics

**Technical Notes:**
- Use Application Insights alerts
- Set alert thresholds based on SLOs
- Include context in alert messages (not just threshold hit)
- Test alert notifications work

---

## Future Enhancements (Backlog)

### Enhancement: Delivery Status Tracking
**Story Points:** 5
**Description:** Add endpoint for Twilio to post delivery status updates and track message delivery states.

---

### Enhancement: Multi-Venue Phone Number Pooling
**Story Points:** 8
**Description:** Optimize costs by using pooled phone numbers with routing by venue.

---

### Enhancement: Additional SMS Providers (Plivo, MessageBird)
**Story Points:** 13
**Description:** Implement additional SMS providers with automatic failover.

---

### Enhancement: Self-Service Consent Management Portal
**Story Points:** 8
**Description:** Customer-facing UI for managing SMS preferences per venue.

---

### Enhancement: Rate Limiting & Throttling
**Story Points:** 5
**Description:** Implement configurable rate limits to avoid hitting provider limits.

---

### Enhancement: Redis Caching for Consent
**Story Points:** 5
**Description:** Add Redis layer to cache frequent consent lookups.

---

## Deployment Strategy

### Multiple Daily Deployments
- **Frequency:** Deploy multiple times per day (main branch auto-deploys to dev)
- **Approval Gate:** Manual approval required for production
- **Rollback:** Via feature flag (no need for container rollback)
- **Feature Flag:** Order Confirmation service checks flag before publishing SMS event
  - Can enable/disable SMS flow per venue without code deployment
  - Gradual rollout: enable for 10% venues, then 50%, then 100%

### GitHub Actions Workflow
1. **On every commit:** Build, unit test, coverage check
2. **On main branch:** Build Docker image, push to ACR, deploy to dev
3. **Manual trigger:** Deploy to production (requires approval)
4. **Production deployment:** Terraform applies infrastructure changes, GitHub Actions updates container

### Terraform Workflow
1. **Local development:** `terraform plan` to validate
2. **On merge to main:** Terraform Cloud applies changes
3. **ACR image updates:** Container Apps pulls latest tagged image
4. **Blue-green deployment:** (future enhancement) for zero-downtime updates

---

## Success Metrics

- **Deployment Frequency:** >1x per day
- **Delivery Rate:** >95% of SMS sent successfully
- **Performance:** P95 send latency < 3 seconds
- **Reliability:** 99.9% uptime
- **Compliance:** 0 SMS sent to opted-out users
- **Cost:** <$0.01 per SMS (Twilio + infrastructure)
- **Lead Time:** <1 hour from code commit to production
- **MTTR (Mean Time To Recovery):** <5 minutes via feature flag

---

## Dependencies & Prerequisites

**Before Sprint 1:**
- [ ] GitHub repository created
- [ ] Azure subscription and resource group provisioned
- [ ] ACR (Azure Container Registry) created
- [ ] Terraform Cloud account (or Azure Storage backend) for state
- [ ] Key Vault created (dev and prod)
- [ ] Service Bus namespace with topic (dev and prod)
- [ ] SQL Server instances (dev and prod)
- [ ] Twilio account with test and production phone numbers
- [ ] Datadog account (optional)
- [ ] GitHub Actions secrets configured: AZURE_CREDENTIALS, TWILIO_ACCOUNT_SID, etc.

---

## Risk Register

| Risk | Impact | Mitigation |
|------|--------|------------|
| Multiple deployments = more production incidents | High | Feature flag enables quick rollback, comprehensive testing, monitoring |
| Database schema drift | Medium | Versioned migrations, preview environment testing |
| Service Bus message backlog | Medium | Auto-scaling configured, load testing in sprint 11 |
| Twilio API rate limits | Medium | Rate limiting implemented, monitor usage |
| Key Vault secret rotation | Low | Automatic rotation policy configured |
