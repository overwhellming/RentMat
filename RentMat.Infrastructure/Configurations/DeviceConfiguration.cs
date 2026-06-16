using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentMat.Core.Models;

namespace RentMat.Infrastructure.Configurations;

public class DeviceConfiguration : IEntityTypeConfiguration<Device>
{
    public void Configure(EntityTypeBuilder<Device> b)
    {
        b.Property(d => d.Name)
            .HasMaxLength(100);
        b.Property(d => d.HourRentPrice)
            .HasPrecision(18, 2);
        b.Property(d => d.Status)
            .HasConversion<string>()
            .HasMaxLength(50);
        
        b.HasOne(d => d.Category)
            .WithMany()
            .HasForeignKey(d => d.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}