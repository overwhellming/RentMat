using Microsoft.EntityFrameworkCore;
using RentMat.Application.Common;
using RentMat.Application.Exceptions;
using RentMat.Application.Exceptions.Booking;
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
            throw new BookingForDeviceNotFoundException(deviceId);

        if (activeBooking.UserId != userId)
            throw new BookingAccessDeniedException();
        
        var completedAt = DateTimeOffset.UtcNow;

        activeBooking.EndDate = completedAt;
        activeBooking.Status = BookingStatus.Completed;
        activeBooking.Device.Status = DeviceStatus.Available;
        
        await _db.SaveChangesAsync(cancellationToken);
        await _cache.RemoveByTagAsync(CacheTags.Bookings);
    }
}