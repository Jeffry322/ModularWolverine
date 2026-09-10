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

        builder.Property<string>("_imei")
            .HasMaxLength(15)
            .IsRequired()
            .HasColumnName("imei");
        
        builder.HasIndex("_imei")
            .IsUnique();
        
        builder.Property<string>("_name")
            .HasMaxLength(255)
            .IsRequired(false)
            .HasColumnName("name");
        
        builder.Property<DateTime>("_createdAtUtc")
            .IsRequired()
            .HasColumnName("created_at_utc");

        builder.Property<DeviceState>("state")
            .HasConversion<string>();
    }
}   