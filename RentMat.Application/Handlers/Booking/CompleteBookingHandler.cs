using Microsoft.EntityFrameworkCore;
using RentMat.Application.Exceptions;
using RentMat.Core.Enums;
using RentMat.Infrastructure.Data;
using ZiggyCreatures.Caching.Fusion;

namespace RentMat.Application.Handlers.Booking;

public class CompleteBookingHandler
{
    private readonly AppDbContext _db;
    private IFusionCache _cache;

    public CompleteBookingHandler(AppDbContext db, IFusionCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task Handle(int deviceId, int userId, CancellationToken cancellationToken)
    {
        var activeBooking =
            await _db.Bookings
                .Include(b => b.User)
                .Include(b => b.Device)
                .FirstOrDefaultAsync(b => b.DeviceId == deviceId && b.Status == BookingStatus.Active,
                    cancellationToken);

        if (activeBooking == null)
            throw new ActiveBookingNotFoundException(deviceId);

        if (activeBooking.UserId != userId)
            throw new BookingAccessDeniedException();
        
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
        await _cache.RemoveByTagAsync("bookings");
    }
}