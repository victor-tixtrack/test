using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmsService.Domain.Entities;

namespace SmsService.Domain.Configurations;

public class VenuePhoneNumberConfiguration : IEntityTypeConfiguration<VenuePhoneNumber>
{
    public void Configure(EntityTypeBuilder<VenuePhoneNumber> builder)
    {
        builder.ToTable("VenuePhoneNumber");

        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).ValueGeneratedOnAdd();

        builder.Property(v => v.VenueId).IsRequired();

        builder.Property(v => v.PhoneNumber).IsRequired().HasMaxLength(20);
        builder.HasIndex(v => v.PhoneNumber);

        builder.Property(v => v.ProviderId).IsRequired().HasMaxLength(100);

        builder.Property(v => v.SmsProviderId).IsRequired();

        builder.Property(v => v.Status).IsRequired();

        builder.Property(v => v.AssignedAt).IsRequired().HasColumnType("datetime2");

        builder.Property(v => v.ReleasedAt).HasColumnType("datetime2");

        builder.Property(v => v.CreatedAt).IsRequired().HasColumnType("datetime2");

        builder.Property(v => v.UpdatedAt).IsRequired().HasColumnType("datetime2");

        // Unique constraint: one active phone number per venue per provider
        builder
            .HasIndex(v => new { v.VenueId, v.SmsProviderId })
            .IsUnique()
            .HasFilter("[Status] = 1");

        // Foreign key
        builder
            .HasOne(v => v.SmsProvider)
            .WithMany()
            .HasForeignKey(v => v.SmsProviderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
