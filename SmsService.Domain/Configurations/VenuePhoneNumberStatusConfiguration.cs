using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmsService.Domain.Entities;

namespace SmsService.Domain.Configurations;

public class VenuePhoneNumberStatusConfiguration : IEntityTypeConfiguration<VenuePhoneNumberStatus>
{
    public void Configure(EntityTypeBuilder<VenuePhoneNumberStatus> builder)
    {
        builder.ToTable("VenuePhoneNumberStatuses");

        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).ValueGeneratedOnAdd();

        builder.Property(v => v.Name).IsRequired().HasMaxLength(50);

        builder.HasIndex(v => v.Name).IsUnique();

        builder.Property(v => v.IsActive).IsRequired().HasDefaultValue(true);

        builder.Property(v => v.CreatedAt).IsRequired().HasColumnType("datetime2");

        // Seed data
        builder.HasData(
            new VenuePhoneNumberStatus
            {
                Id = 1,
                Name = "active",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
            },
            new VenuePhoneNumberStatus
            {
                Id = 2,
                Name = "inactive",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
            },
            new VenuePhoneNumberStatus
            {
                Id = 3,
                Name = "released",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
            }
        );
    }
}
