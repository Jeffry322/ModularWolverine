using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModularWolverine.Modules.Devices.Domain.Devices;

namespace ModularWolverine.Modules.Devices.Infrastructure.Database.EntityConfigurations;

internal sealed class DeviceConfiguration : IEntityTypeConfiguration<Device>
{
    public void Configure(EntityTypeBuilder<Device> builder)
    {
        builder.ToTable("devices");
        
        builder.HasKey(x => x.Id);

        builder.Property(d => d.Imei)
            .HasMaxLength(15)
            .IsRequired()
            .HasColumnName("imei");
        
        builder.HasIndex(d => d.Imei)
            .IsUnique();
        
        builder.Property(d => d.Name)
            .HasMaxLength(255)
            .IsRequired(false)
            .HasColumnName("name");
        
        builder.Property(d => d.CreatedAtUtc)
            .IsRequired()
            .HasColumnName("created_at_utc");

        builder.Property(d => d.State)
            .HasConversion<string>();
    }
}   