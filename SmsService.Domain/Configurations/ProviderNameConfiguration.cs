using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmsService.Domain.Entities;

namespace SmsService.Domain.Configurations;

public class ProviderNameConfiguration : IEntityTypeConfiguration<ProviderName>
{
    public void Configure(EntityTypeBuilder<ProviderName> builder)
    {
        builder.ToTable("ProviderNames");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedOnAdd();

        builder.Property(p => p.Name).IsRequired().HasMaxLength(50);

        builder.HasIndex(p => p.Name).IsUnique();

        builder.Property(p => p.IsActive).IsRequired().HasDefaultValue(true);

        builder.Property(p => p.CreatedAt).IsRequired().HasColumnType("datetime2");

        // Seed data
        builder.HasData(
            new ProviderName
            {
                Id = 1,
                Name = "twilio",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
            }
        );
    }
}
