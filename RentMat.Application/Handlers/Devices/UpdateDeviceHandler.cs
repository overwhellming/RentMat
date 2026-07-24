using Microsoft.EntityFrameworkCore;
using RentMat.Application.Common;
using RentMat.Application.DTOs.Device;
using RentMat.Application.Exceptions.Devices;
using RentMat.Core.Enums;
using RentMat.Infrastructure.Data;
using ZiggyCreatures.Caching.Fusion;

namespace RentMat.Application.Handlers.Devices;

public class UpdateDeviceHandler
{
    private readonly IFusionCache _cache;
    private readonly AppDbContext _db;

    public UpdateDeviceHandler(AppDbContext db, IFusionCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task Handle(int deviceId, DeviceUpdateDto dto, CancellationToken cancellationToken)
    {
        var categoryExists = await _db.DeviceCategories
            .AnyAsync(c => c.Id == dto.CategoryId, cancellationToken);
        if (!categoryExists)
            throw new DeviceCategoryNotFoundException(dto.CategoryId);

        var device = await _db.Devices
            .FirstOrDefaultAsync(d => d.Id == deviceId, cancellationToken);
        if (device == null)
            throw new DeviceNotFoundException(deviceId);

        var hasActiveBookings = await _db.Bookings.AnyAsync(b =>
                b.DeviceId == deviceId && (b.Status == BookingStatus.Active || b.Status == BookingStatus.Created),
            cancellationToken);
        hasActiveBookings = hasActiveBookings || 
                            (await _db.Devices.FindAsync(device.Id, cancellationToken))!
                            .Status == DeviceStatus.Rented;
        
        if (hasActiveBookings)
            throw new DeviceIsBookedException(deviceId);

        device.Name = dto.Name;
        device.HourRentPrice = dto.HourRentPrice;
        device.CategoryId = dto.CategoryId;

        await _db.SaveChangesAsync(cancellationToken);

        await _cache.RemoveByTagAsync(CacheTags.Devices);
    }
}