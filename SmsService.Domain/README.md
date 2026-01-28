# SmsService.Domain

Entity Framework Core domain layer for SMS Service. This project contains entities, DbContext, and entity configurations. It is designed to be standalone and can be referenced by both the main sms-service application and the separate migration repository.

## Structure

```
SmsService.Domain/
├── Data/
│   └── SmsDbContext.cs              # EF Core DbContext
├── Entities/
│   ├── ProviderName.cs              # Enum: twilio, plivo
│   ├── VenuePhoneNumberStatus.cs    # Enum: active, inactive, released
│   ├── SmsConsentStatus.cs          # Enum: opted_in, opted_out
│   ├── ConsentSource.cs             # Enum: checkout, account_settings, support_request
│   ├── SmsSendHistoryStatus.cs      # Enum: sent, failed, skipped_no_consent, blocked_opted_out
│   ├── VenuePhoneNumber.cs          # Business entity: phone assignments
│   ├── SmsConsent.cs                # Business entity: consent tracking
│   └── SmsSendHistory.cs            # Business entity: audit history
├── ValueObjects/
│   └── PhoneNumber.cs               # E.164 phone number validation
└── Configurations/
    └── *Configuration.cs            # Fluent API entity configurations
```

## Design Decisions

### Standalone Project
- **No dependencies** on SmsService.Core or SmsService.Infrastructure
- Only references EF Core packages
- Can be sparse-checked out by migration repository

### DbContext Location
- DbContext lives in Domain (not Infrastructure) to enable migration repository to:
  - Generate migrations using `dotnet ef migrations add`
  - Run validation tests comparing entities to `__ModelSnapshot`
  - Apply migrations without needing Infrastructure dependencies

### Value Objects
- `PhoneNumber` value object provides E.164 validation
- Used by both domain entities and application services
- Stored as string in database (`PhoneNumberValue` column)
- Exposed as computed property (`PhoneNumber`) for type safety

### Entity Design
- All entities use `int` primary keys (except `SmsSendHistory` uses `long`)
- Foreign keys reference enum tables for type safety
- Navigation properties enable eager loading
- Seed data included in configurations for enum tables
- Timestamps use `datetime2` type for precision

## Usage

### Registering DbContext

```csharp
services.AddDbContext<SmsDbContext>(options =>
    options.UseSqlServer(
        configuration.GetConnectionString("SmsDatabase"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()
    )
);
```

### Connection String

```json
{
  "ConnectionStrings": {
    "SmsDatabase": "Server=localhost,3433;Database=SmsService;User Id=sa;Password=DevPassword123!;TrustServerCertificate=true;"
  }
}
```

## Migration Workflow

### Local Development (sms-service repo)
1. Modify entities in `SmsService.Domain/`
2. Build to verify: `dotnet build SmsService.Domain`
3. Commit changes to sms-service repo

### Migration Generation (sms-service-db repo)
1. Clone sms-service-db adjacent to sms-service
2. `cd sms-service-db`
3. Sparse checkout will pull latest Domain on CI
4. Generate migration: `dotnet ef migrations add MigrationName --project SmsService.Migrations.csproj`
5. Review generated migration files
6. Commit and push to sms-service-db repo

### CI/CD (sms-service-db repo)
- PR workflow: Sparse checkout Domain, run validation tests
- Main workflow: Apply migrations to Azure SQL Database

## Next Steps

1. **Setup sms-service-db repository** with:
   - `.csproj` referencing Domain
   - Validation test project
   - GitHub Actions for sparse checkout and migration application

2. **Document local developer workflow**:
   - How to clone both repos
   - How to generate migrations locally
   - How to test migrations before committing

3. **Implement repository interfaces** in sms-service:
   - `IConsentRepository` in Core
   - `ConsentRepository` in Infrastructure using `SmsDbContext`
