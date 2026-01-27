using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmsService.Domain.Entities;

namespace SmsService.Domain.Configurations;

public class SmsSendHistoryConfiguration : IEntityTypeConfiguration<SmsSendHistory>
{
    public void Configure(EntityTypeBuilder<SmsSendHistory> builder)
    {
        builder.ToTable("SmsSendHistories");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedOnAdd();

        builder.Property(s => s.OrderId).IsRequired();

        builder.Property(s => s.VenueId).IsRequired();

        builder.Property(s => s.VenuePhoneNumberId).IsRequired();

        builder.Property(s => s.CustomerId).IsRequired();

        builder
            .Property(s => s.CustomerPhoneNumberValue)
            .IsRequired()
            .HasMaxLength(20)
            .HasColumnName("CustomerPhoneNumber");

        builder.Property(s => s.Message).IsRequired().HasMaxLength(500);

        builder.Property(s => s.ProviderNameId).IsRequired();

        builder.Property(s => s.StatusId).IsRequired();

        builder.Property(s => s.ProviderMessageId).HasMaxLength(100);

        builder.Property(s => s.ErrorCode).HasMaxLength(50);

        builder.Property(s => s.CreatedAt).IsRequired().HasColumnType("datetime2");

        // Indexes for compliance and time-range queries
        builder.HasIndex(s => s.OrderId);
        builder.HasIndex(s => s.VenueId);
        builder
            .HasIndex(s => new { s.VenueId, s.CreatedAt })
            .HasDatabaseName("IX_SmsSendHistories_VenueId_CreatedAt");
        builder
            .HasIndex(s => new { s.CustomerPhoneNumberValue, s.VenueId })
            .HasDatabaseName("IX_SmsSendHistories_CustomerPhoneNumber_VenueId");

        // Clustered index on CreatedAt for time-range queries
        builder.HasIndex(s => s.CreatedAt).HasDatabaseName("IX_SmsSendHistories_CreatedAt");

        // Relationships
        builder
            .HasOne(s => s.VenuePhoneNumber)
            .WithMany(v => v.SmsSendHistories)
            .HasForeignKey(s => s.VenuePhoneNumberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(s => s.ProviderName)
            .WithMany(p => p.SmsSendHistories)
            .HasForeignKey(s => s.ProviderNameId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(s => s.Status)
            .WithMany(st => st.SmsSendHistories)
            .HasForeignKey(s => s.StatusId)
            .OnDelete(DeleteBehavior.Restrict);

        // Ignore computed property
        builder.Ignore(s => s.CustomerPhoneNumber);
    }
}
