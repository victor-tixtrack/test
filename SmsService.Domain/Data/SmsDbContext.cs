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

    // SMS Providers
    public DbSet<SmsProvider> SmsProviders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all IEntityTypeConfiguration classes from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SmsDbContext).Assembly);
    }
}
