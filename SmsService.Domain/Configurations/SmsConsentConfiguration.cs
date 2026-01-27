using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmsService.Domain.Entities;

namespace SmsService.Domain.Configurations;

public class SmsConsentConfiguration : IEntityTypeConfiguration<SmsConsent>
{
    public void Configure(EntityTypeBuilder<SmsConsent> builder)
    {
        builder.ToTable("SmsConsents");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedOnAdd();

        builder.Property(s => s.VenueId).IsRequired();

        builder
            .Property(s => s.PhoneNumberValue)
            .IsRequired()
            .HasMaxLength(20)
            .HasColumnName("PhoneNumber");

        builder.Property(s => s.StatusId).IsRequired();

        builder.Property(s => s.ConsentSourceId);

        builder.Property(s => s.InitialConsentAt).HasColumnType("datetime2");

        builder.Property(s => s.OptedOutAt).HasColumnType("datetime2");

        builder.Property(s => s.OptedInAt).HasColumnType("datetime2");

        builder.Property(s => s.CreatedAt).IsRequired().HasColumnType("datetime2");

        builder.Property(s => s.UpdatedAt).IsRequired().HasColumnType("datetime2");

        // Indexes
        builder.HasIndex(s => s.VenueId);
        builder.HasIndex(s => s.PhoneNumberValue).HasDatabaseName("IX_SmsConsents_PhoneNumber");

        // Unique constraint: one consent record per venue + phone
        builder
            .HasIndex(s => new { s.VenueId, s.PhoneNumberValue })
            .IsUnique()
            .HasDatabaseName("IX_SmsConsents_VenueId_PhoneNumber");

        // Relationships
        builder
            .HasOne(s => s.Status)
            .WithMany(st => st.SmsConsents)
            .HasForeignKey(s => s.StatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(s => s.ConsentSource)
            .WithMany(cs => cs.SmsConsents)
            .HasForeignKey(s => s.ConsentSourceId)
            .OnDelete(DeleteBehavior.Restrict);

        // Ignore computed property
        builder.Ignore(s => s.PhoneNumber);
    }
}
