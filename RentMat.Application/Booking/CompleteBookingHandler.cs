using Microsoft.EntityFrameworkCore;
using RentMat.Application.Booking.Exceptions;
using RentMat.Core.Enums;
using RentMat.Infrastructure.Data;

namespace RentMat.Application.Booking;

public class CompleteBookingHandler
{
    private readonly AppDbContext _db;

    public CompleteBookingHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task Handle(int deviceId, CancellationToken cancellationToken)
    {
        var activeBooking =
            await _db.Bookings
                .Include(b => b.User)
                .Include(b => b.Device)
                .FirstOrDefaultAsync(b => b.DeviceId == deviceId && b.Status == BookingStatus.Active,
                    cancellationToken);

        if (activeBooking == null)
            throw new ActiveBookingNotFoundException(deviceId);

        var completedAt = DateTimeOffset.UtcNow;
        var actualHours = Math.Max(0m, (decimal)(completedAt - activeBooking.StartDate).TotalMinutes / 60m);
        var actualPrice = Math.Round(actualHours * activeBooking.Device.HourRentPrice, 2,
            MidpointRounding.AwayFromZero);
        var refund = activeBooking.TotalPrice - actualPrice;

        activeBooking.EndDate = completedAt;
        activeBooking.Status = BookingStatus.Completed;
        if (refund > 0)
            activeBooking.User.Balance += refund;
        activeBooking.TotalPrice = actualPrice;

        await _db.SaveChangesAsync();
    }
}