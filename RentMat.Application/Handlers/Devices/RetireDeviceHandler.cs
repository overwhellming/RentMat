using Microsoft.EntityFrameworkCore;
using RentMat.Application.Common;
using RentMat.Application.Exceptions.Devices;
using RentMat.Core.Enums;
using RentMat.Infrastructure.Data;
using ZiggyCreatures.Caching.Fusion;

namespace RentMat.Application.Handlers.Devices;

public class RetireDeviceHandler
{
    private readonly AppDbContext _db;
    private readonly IFusionCache _cache;
    
    public RetireDeviceHandler(AppDbContext db, IFusionCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task Handle(int deviceId, CancellationToken cancellationToken)
    {
        var device = await _db.Devices
            .FirstOrDefaultAsync(d => d.Id == deviceId, cancellationToken);
        if (device == null)
            throw new DeviceNotFoundException(deviceId);

        var hasActiveBookings = await _db.Bookings
            .AnyAsync(
                b => b.DeviceId == deviceId &&
                     (b.Status == BookingStatus.Active || b.Status == BookingStatus.Created),
                cancellationToken);
        if (hasActiveBookings)
            throw new DeviceIsBookedException(deviceId);
        
        device.Status = DeviceStatus.Retired;
        await _db.SaveChangesAsync(cancellationToken);

        await _cache.RemoveByTagAsync(CacheTags.Devices);
    }
}