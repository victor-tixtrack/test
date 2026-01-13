# SMS Notification Service

A vendor-agnostic SMS notification service for sending order confirmation messages, designed to integrate with the existing TixTrack order flow while maintaining flexibility to swap SMS providers.

## Solution Structure

```
SmsService.sln
├── SmsService.Core/                 # Domain models and interfaces (no external dependencies)
│   ├── Models/                      # Domain entities and DTOs
│   ├── Interfaces/                  # Repository and service interfaces
│   └── Services/                    # Domain services
├── SmsService.Infrastructure/       # External services and data access (EF Core, Twilio, etc)
│   ├── Data/                        # DbContext and migrations
│   ├── Repositories/                # Repository implementations
│   ├── Services/                    # Infrastructure services
│   └── Configurations/              # EF configurations
├── SmsService.Api/                  # REST API layer
│   ├── Endpoints/                   # API endpoints
│   ├── Middleware/                  # Custom middleware
│   └── Configuration/               # Dependency injection setup
└── SmsService.Tests/                # Unit and integration tests
    ├── Unit/                        # Unit tests
    └── Integration/                 # Integration tests
```

## Getting Started

### Prerequisites

- .NET 10 SDK
- Docker and Docker Compose
- Git

### Local Development Setup

1. **Clone the repository**
   ```bash
   git clone <repo-url>
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

## Development Workflow

### Setting Up Pre-Commit Hooks

Code formatting with CSharpier runs automatically on commit to maintain consistent style. Setup instructions differ by platform:

#### macOS

1. **Install pre-commit framework**
   ```bash
   brew install pre-commit
   ```

2. **Install git hooks**
   ```bash
   cd sms-service
   pre-commit install
   ```

3. **Verify setup** (optional)
   ```bash
   pre-commit run --all-files
   ```

#### Windows (PowerShell)

1. **Install pre-commit framework using pip**
   ```powershell
   # Option A: Using pipx (recommended)
   pipx install pre-commit
   
   # Option B: Using pip
   pip install pre-commit
   ```

2. **Install git hooks**
   ```powershell
   cd sms-service
   pre-commit install
   ```

3. **Verify setup** (optional)
   ```powershell
   pre-commit run --all-files
   ```

#### Linux

1. **Install pre-commit framework**
   ```bash
   sudo apt-get install python3-pip
   pip3 install pre-commit
   ```

2. **Install git hooks**
   ```bash
   cd sms-service
   pre-commit install
   ```

3. **Verify setup** (optional)
   ```bash
   pre-commit run --all-files
   ```

### What Pre-Commit Hooks Do

When you commit code:
1. **dotnet tool restore** - Ensures CSharpier is installed
2. **CSharpier** - Automatically formats all C# files to project standards

If CSharpier makes changes, the commit will fail. Simply `git add` the formatted files and commit again.

### Troubleshooting Pre-Commit

**Hook not running on commit?**
```bash
# Verify hooks are installed
cat .git/hooks/pre-commit

# Re-install if needed
pre-commit install --install-hooks
```

**CSharpier not found?**
```bash
# Restore .NET tools manually
dotnet tool restore
```

**Want to skip hooks for a commit?**
```bash
git commit --no-verify  # Use with caution!
```

### Standard Development Workflow

1. Create feature branch from `main`
2. Make changes with tests
3. Ensure coverage >80%
4. Commit (pre-commit hooks auto-format code)
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
