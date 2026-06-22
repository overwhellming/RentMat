using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentMat.Core.Constants;
using RentMat.Core.Models;

namespace RentMat.Infrastructure.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> b)
    {
        b.Property(r => r.TotalPrice)
            .HasPrecision(18, 2);
        b.Property(r => r.Status)
            .HasConversion<string>()
            .HasMaxLength(ValidationConstants.BookingStatusMaxLength);
        
        b.ToTable(t =>
        {
            t.HasCheckConstraint(
                "CK_RentalBooking_EndAfterStart",
                $"\"{nameof(Booking.EndDate)}\" > \"{nameof(Booking.StartDate)}\""
            );
            t.HasCheckConstraint(
                "CK_RentalBooking_TotalPrice_NonNegative",
                $"\"{nameof(Booking.TotalPrice)}\" >= 0"
            );
        });
        
        b.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasOne(r => r.Device)
            .WithMany()
            .HasForeignKey(r => r.DeviceId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(r => r.UserId);
        b.HasIndex(r => r.DeviceId);
        b.HasIndex(r => new
        {
            r.DeviceId,
            r.StartDate,
            r.EndDate
        });
    }
}