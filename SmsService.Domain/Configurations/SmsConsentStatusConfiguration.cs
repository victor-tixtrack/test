using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmsService.Domain.Entities;

namespace SmsService.Domain.Configurations;

public class SmsConsentStatusConfiguration : IEntityTypeConfiguration<SmsConsentStatus>
{
    public void Configure(EntityTypeBuilder<SmsConsentStatus> builder)
    {
        builder.ToTable("SmsConsentStatuses");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedOnAdd();

        builder.Property(s => s.Name).IsRequired().HasMaxLength(50);

        builder.HasIndex(s => s.Name).IsUnique();

        builder.Property(s => s.IsActive).IsRequired().HasDefaultValue(true);

        builder.Property(s => s.CreatedAt).IsRequired().HasColumnType("datetime2");

        // Seed data
        builder.HasData(
            new SmsConsentStatus
            {
                Id = 1,
                Name = "opted_in",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
            },
            new SmsConsentStatus
            {
                Id = 2,
                Name = "opted_out",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
            }
        );
    }
}
