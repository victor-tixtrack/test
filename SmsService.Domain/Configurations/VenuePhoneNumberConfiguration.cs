using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmsService.Domain.Entities;

namespace SmsService.Domain.Configurations;

public class VenuePhoneNumberConfiguration : IEntityTypeConfiguration<VenuePhoneNumber>
{
    public void Configure(EntityTypeBuilder<VenuePhoneNumber> builder)
    {
        builder.ToTable("VenuePhoneNumbers");

        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).ValueGeneratedOnAdd();

        builder.Property(v => v.VenueId).IsRequired();

        builder
            .Property(v => v.PhoneNumberValue)
            .IsRequired()
            .HasMaxLength(20)
            .HasColumnName("PhoneNumber");

        builder.Property(v => v.ProviderId).IsRequired().HasMaxLength(100);

        builder.Property(v => v.ProviderNameId).IsRequired();

        builder.Property(v => v.StatusId).IsRequired();

        builder.Property(v => v.AssignedAt).IsRequired().HasColumnType("datetime2");

        builder.Property(v => v.ReleasedAt).HasColumnType("datetime2");

        builder.Property(v => v.CreatedAt).IsRequired().HasColumnType("datetime2");

        builder.Property(v => v.UpdatedAt).IsRequired().HasColumnType("datetime2");

        // Indexes
        builder.HasIndex(v => v.VenueId);
        builder
            .HasIndex(v => v.PhoneNumberValue)
            .HasDatabaseName("IX_VenuePhoneNumbers_PhoneNumber");

        // Unique constraint: one active provider per venue
        builder
            .HasIndex(v => new { v.VenueId, v.ProviderNameId })
            .IsUnique()
            .HasFilter("[StatusId] = 1")
            .HasDatabaseName("IX_VenuePhoneNumbers_VenueId_ProviderNameId_Active");

        // Relationships
        builder
            .HasOne(v => v.ProviderName)
            .WithMany(p => p.VenuePhoneNumbers)
            .HasForeignKey(v => v.ProviderNameId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(v => v.Status)
            .WithMany(s => s.VenuePhoneNumbers)
            .HasForeignKey(v => v.StatusId)
            .OnDelete(DeleteBehavior.Restrict);

        // Ignore computed property
        builder.Ignore(v => v.PhoneNumber);
    }
}
