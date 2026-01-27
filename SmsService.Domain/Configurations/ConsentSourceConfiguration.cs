using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmsService.Domain.Entities;

namespace SmsService.Domain.Configurations;

public class ConsentSourceConfiguration : IEntityTypeConfiguration<ConsentSource>
{
    public void Configure(EntityTypeBuilder<ConsentSource> builder)
    {
        builder.ToTable("ConsentSources");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedOnAdd();

        builder.Property(c => c.Name).IsRequired().HasMaxLength(50);

        builder.HasIndex(c => c.Name).IsUnique();

        builder.Property(c => c.IsActive).IsRequired().HasDefaultValue(true);

        builder.Property(c => c.CreatedAt).IsRequired().HasColumnType("datetime2");

        // Seed data
        builder.HasData(
            new ConsentSource
            {
                Id = 1,
                Name = "checkout",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
            },
            new ConsentSource
            {
                Id = 2,
                Name = "account_settings",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
            },
            new ConsentSource
            {
                Id = 3,
                Name = "support_request",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
            }
        );
    }
}
