using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmsService.Domain.Entities;

namespace SmsService.Domain.Configurations;

public class SmsProviderConfiguration : IEntityTypeConfiguration<SmsProvider>
{
    public void Configure(EntityTypeBuilder<SmsProvider> builder)
    {
        builder.ToTable("SmsProvider");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedOnAdd();

        builder.Property(p => p.Name).IsRequired().HasMaxLength(50);

        builder.HasIndex(p => p.Name).IsUnique();

        builder.Property(p => p.IsActive).IsRequired().HasDefaultValue(true);

        builder.Property(p => p.CreatedAt).IsRequired().HasColumnType("datetime2");
    }
}
