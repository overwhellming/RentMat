using MediatR;
using Microsoft.EntityFrameworkCore;
using RentMat.Application.Commands.Devices;
using RentMat.Application.Common;
using RentMat.Application.DTOs.Device;
using RentMat.Application.Exceptions.Devices;
using RentMat.Core.Models;
using RentMat.Infrastructure.Data;
using ZiggyCreatures.Caching.Fusion;

namespace RentMat.Application.Handlers.Devices;

public class CreateDeviceCommandHandler : IRequestHandler<CreateDeviceCommand, DeviceResponseDto>
{
    private readonly IFusionCache _cache;
    private readonly AppDbContext _db;

    public CreateDeviceCommandHandler(AppDbContext db, IFusionCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<DeviceResponseDto> Handle(CreateDeviceCommand request, CancellationToken cancellationToken)
    {
        var categoryName = await _db.DeviceCategories
            .AsNoTracking()
            .Where(c => c.Id == request.CategoryId)
            .Select(c => c.Name)
            .SingleOrDefaultAsync(cancellationToken);

        if (categoryName == null)
            throw new DeviceCategoryNotFoundException(request.CategoryId);

        var device = new Device
        {
            Name = request.Name,
            HourRentPrice = request.HourRentPrice,
            CategoryId = request.CategoryId
        };

        _db.Devices.Add(device);
        await _db.SaveChangesAsync(cancellationToken);

        await _cache.RemoveByTagAsync(CacheTags.Devices);

        return new DeviceResponseDto(device.Id, device.Name, device.HourRentPrice, categoryName,
            device.Status.ToString());
    }
}