using Microsoft.EntityFrameworkCore;
using SmsService.Domain.Entities;

namespace SmsService.Domain.Data;

/// <summary>
/// Entity Framework DbContext for SMS Service database
/// </summary>
public class SmsDbContext : DbContext
{
    public SmsDbContext(DbContextOptions<SmsDbContext> options)
        : base(options) { }

    // Enum/Lookup Tables
    public DbSet<ProviderName> ProviderNames { get; set; }
    public DbSet<VenuePhoneNumberStatus> VenuePhoneNumberStatuses { get; set; }
    public DbSet<SmsConsentStatus> SmsConsentStatuses { get; set; }
    public DbSet<ConsentSource> ConsentSources { get; set; }
    public DbSet<SmsSendHistoryStatus> SmsSendHistoryStatuses { get; set; }

    // Business Tables
    public DbSet<VenuePhoneNumber> VenuePhoneNumbers { get; set; }
    public DbSet<SmsConsent> SmsConsents { get; set; }
    public DbSet<SmsSendHistory> SmsSendHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all IEntityTypeConfiguration classes from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SmsDbContext).Assembly);
    }
}
