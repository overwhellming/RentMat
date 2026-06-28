using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentMat.Application.Common;
using RentMat.Application.DTOs.Device;
using RentMat.Application.Exceptions.Devices;
using RentMat.Infrastructure.Data;
using ZiggyCreatures.Caching.Fusion;

namespace RentMat.Application.Handlers.Devices;

public class GetDeviceByIdHandler
{
    private readonly IFusionCache _cache;
    private readonly AppDbContext _db;
    private readonly ILogger<GetDeviceByIdHandler> _logger;

    public GetDeviceByIdHandler(AppDbContext db, IFusionCache cache, ILogger<GetDeviceByIdHandler> logger)
    {
        _db = db;
        _cache = cache;
        _logger = logger;
    }

    public async Task<DeviceResponseDto> Handle(int deviceId, CancellationToken cancellationToken)
    {
        var cacheKey = $"devices:id:{deviceId}";

        return await _cache.GetOrSetAsync<DeviceResponseDto>(
            cacheKey,
            async (ctx, ct) =>
            {
                _logger.LogDebug("Cache miss for key {CacheKey}", cacheKey);

                var device = await _db.Devices
                    .AsNoTracking()
                    .Where(d => d.Id == deviceId)
                    .Select(d => new DeviceResponseDto(
                        d.Id,
                        d.Name,
                        d.HourRentPrice,
                        d.Category.Name,
                        d.Status.ToString()
                    ))
                    .SingleOrDefaultAsync(ct);

                return device ?? throw new DeviceNotFoundException(deviceId);
            },
            tags: [CacheTags.Devices],
            token: cancellationToken
        );
    }
}