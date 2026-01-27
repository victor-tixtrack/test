# SMS Notification Service

A vendor-agnostic SMS notification service for sending order confirmation messages, designed to integrate with the existing TixTrack order flow while maintaining flexibility to swap SMS providers.

## Solution Structure

```
SmsService.sln
├── SmsService.Domain/               # Domain entities and business logic (DDD approach)
│   ├── Entities/                    # Domain entities (e.g., SmsProvider)
│   ├── Configurations/              # EF Core entity configurations
│   └── Data/                        # DbContext
├── SmsService.Core/                 # Application core and interfaces
│   ├── Models/                      # DTOs and view models
│   ├── Interfaces/                  # Repository and service interfaces
│   └── Services/                    # Application services
├── SmsService.Infrastructure/       # External services and data access implementations
│   ├── Repositories/                # Repository implementations
│   ├── Services/                    # Infrastructure services (Twilio, etc)
│   └── Configurations/              # Infrastructure configuration
├── SmsService.Api/                  # REST API layer
│   ├── Endpoints/                   # API endpoints
│   ├── Middleware/                  # Custom middleware
│   └── Configuration/               # Dependency injection setup
├── SmsService.Tests/                # Unit and integration tests
│   ├── Unit/                        # Unit tests
│   └── Integration/                 # Integration tests
└── SmsService.Core.Database.Tests/  # Database migration validation tests
```

## Getting Started

### Prerequisites

- .NET 10 SDK
- Docker and Docker Compose
- Git

### Local Development Setup

1. **Clone the repository**
   ```bash
   git clone <sms-service-repo-url>
   git clone <sms-service-db-repo-url>
   cd sms-service
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Run with Docker Compose** (no Azure infrastructure needed)
   ```bash
   docker-compose up
   ```
   
   The API will be available at `http://localhost:5000`

4. **Run locally without Docker**
   ```bash
   cd SmsService.Api
   dotnet run
   ```

### Health Check Endpoints

- `GET /healthz/live` - Liveness probe (always healthy if app is running)
- `GET /healthz/ready` - Readiness probe (healthy when ready to accept requests)

### Running Tests

```bash
dotnet test
```

## Architecture Decisions

See [sms-architecture-design.md](sms-architecture-design.md) for detailed architecture documentation.

### Key Principles

- **Event-Driven**: Uses Azure Service Bus for decoupling
- **Vendor-Agnostic**: Port/Adapter pattern for SMS provider abstraction
- **Multi-Tenant**: Each venue gets dedicated phone number
- **Consent-First**: Check consent before publishing to service bus

## Database Separation

Database migrations are managed in a separate Git repository (`sms-service-db`) to enable independent deployment cycles. This repository must be cloned as a sibling directory alongside the sms-service repository.

This separation supports zero-downtime deployments by allowing database schema changes to be deployed independently from application code changes. The architecture supports roll-forward deployments:

1. Deploy database migrations (adds new columns/tables without breaking existing code)
2. Deploy domain model code with new properties (new properties with NotMapped attributes)

We run database tests on each PR to ensure PRs are rolling forward safely, and not making unexpected breaking changes.

## Development Workflow

### Making an EntityFramework Change

**Zero Downtime Deployment**

1. PR A: Add property to entity with `[NotMapped]` attribute
2. PR B: Generate migration: `cd ../sms-service-db/Migrations && USE_REAL_DB=true dotnet ef migrations add AddMyProperty`
3. Note the Deploy order of PR A and PR B does not matter, both are safe to do without the other!
3. PR C: Remove `[NotMapped]` attribute → (property now mapped to column)


### Standard Development Workflow

1. Create feature branch from `main`
2. Make changes with tests
3. Ensure coverage >80%
4. Commit
5. Push to GitHub (triggers CI/CD)
6. Create Pull Request
7. Code review and merge to `main`
8. Automatic deploy to dev environment
9. Manual approval to deploy to production

## Deployment

### CI/CD Pipeline

GitHub Actions automatically:
- Builds on every commit
- Runs unit tests
- Checks coverage (must be >80%)
- Builds Docker image
- Pushes to Azure Container Registry
- Deploys to dev environment (auto)
- Requires manual approval for production

### Infrastructure

See Terraform configuration in `/terraform` directory for:
- Azure Container Apps
- Azure Container Registry
- Key Vault
- Log Analytics

## Documentation

- [High-Level Architecture](sms-architecture-high-level.md)
- [Detailed Design](sms-architecture-design.md)
- [Implementation Plan](implementation-plan-jira-tickets.md)

## Contributing

- Follow clean architecture principles
- Write tests for all new features
- Use meaningful commit messages
- Ensure code coverage stays >80%

## Support

For issues or questions, contact the development team.
