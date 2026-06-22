using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentMat.Core.Constants;
using RentMat.Core.Models;

namespace RentMat.Infrastructure.Configurations;

public class DeviceCategoryConfiguration : IEntityTypeConfiguration<DeviceCategory>
{
    public void Configure(EntityTypeBuilder<DeviceCategory> b)
    {
        b.Property(c => c.Name)
            .HasMaxLength(ValidationConstants.DeviceCategoryMaxLength);

        b.HasIndex(c => c.Name)
            .IsUnique();
    }
}