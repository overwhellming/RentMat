using MediatR;
using Microsoft.EntityFrameworkCore;
using RentMat.Application.Commands.Devices;
using RentMat.Application.Common;
using RentMat.Application.Exceptions.Devices;
using RentMat.Core.Enums;
using RentMat.Infrastructure.Data;
using ZiggyCreatures.Caching.Fusion;

namespace RentMat.Application.Handlers.Devices;

public class UpdateDeviceCommandHandler : IRequestHandler<UpdateDeviceCommand>
{
    private readonly IFusionCache _cache;
    private readonly AppDbContext _db;

    public UpdateDeviceCommandHandler(AppDbContext db, IFusionCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task Handle(UpdateDeviceCommand command, CancellationToken cancellationToken)
    {
        var categoryExists = await _db.DeviceCategories
            .AnyAsync(c => c.Id == command.CategoryId, cancellationToken);
        if (!categoryExists)
            throw new DeviceCategoryNotFoundException(command.CategoryId);

        var device = await _db.Devices
            .FirstOrDefaultAsync(d => d.Id == command.Id, cancellationToken);
        if (device == null)
            throw new DeviceNotFoundException(command.Id);

        var hasActiveBookings = await _db.Bookings.AnyAsync(b =>
                b.DeviceId == command.Id && (b.Status == BookingStatus.Active || b.Status == BookingStatus.Created),
            cancellationToken);
        hasActiveBookings = hasActiveBookings ||
                            (await _db.Devices.FindAsync(device.Id, cancellationToken))!
                            .Status == DeviceStatus.Rented;

        if (hasActiveBookings)
            throw new DeviceIsBookedException(command.Id);

        device.Name = command.Name;
        device.HourRentPrice = command.HourRentPrice;
        device.CategoryId = command.CategoryId;

        await _db.SaveChangesAsync(cancellationToken);

        await _cache.RemoveByTagAsync(CacheTags.Devices);
    }
}