using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmsService.Domain.Entities;

namespace SmsService.Domain.Configurations;

public class SmsSendHistoryStatusConfiguration : IEntityTypeConfiguration<SmsSendHistoryStatus>
{
    public void Configure(EntityTypeBuilder<SmsSendHistoryStatus> builder)
    {
        builder.ToTable("SmsSendHistoryStatuses");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedOnAdd();

        builder.Property(s => s.Name).IsRequired().HasMaxLength(30);

        builder.HasIndex(s => s.Name).IsUnique();

        builder.Property(s => s.IsActive).IsRequired().HasDefaultValue(true);

        builder.Property(s => s.CreatedAt).IsRequired().HasColumnType("datetime2");

        // Seed data
        builder.HasData(
            new SmsSendHistoryStatus
            {
                Id = 1,
                Name = "sent",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
            },
            new SmsSendHistoryStatus
            {
                Id = 2,
                Name = "failed",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
            },
            new SmsSendHistoryStatus
            {
                Id = 3,
                Name = "skipped_no_consent",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
            },
            new SmsSendHistoryStatus
            {
                Id = 4,
                Name = "blocked_opted_out",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
            }
        );
    }
}
