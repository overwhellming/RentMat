using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentMat.Application.Common;
using RentMat.Application.DTOs.Device;
using RentMat.Application.Exceptions.Devices;
using RentMat.Application.Queries.Devices;
using RentMat.Infrastructure.Data;
using ZiggyCreatures.Caching.Fusion;

namespace RentMat.Application.Handlers.Devices;

public class GetDeviceByIdQueryHandler : IRequestHandler<GetDeviceByIdQuery, DeviceResponseDto>
{
    private readonly IFusionCache _cache;
    private readonly AppDbContext _db;
    private readonly ILogger<GetDeviceByIdQueryHandler> _logger;

    public GetDeviceByIdQueryHandler(AppDbContext db, IFusionCache cache, ILogger<GetDeviceByIdQueryHandler> logger)
    {
        _db = db;
        _cache = cache;
        _logger = logger;
    }

    public async Task<DeviceResponseDto> Handle(GetDeviceByIdQuery query, CancellationToken cancellationToken)
    {
        var cacheKey = $"devices:id:{query.Id}";

        return await _cache.GetOrSetAsync<DeviceResponseDto>(
            cacheKey,
            async (ctx, ct) =>
            {
                _logger.LogDebug("Cache miss for key {CacheKey}", cacheKey);

                var device = await _db.Devices
                    .AsNoTracking()
                    .Where(d => d.Id == query.Id)
                    .Select(d => new DeviceResponseDto(
                        d.Id,
                        d.Name,
                        d.HourRentPrice,
                        d.Category.Name,
                        d.Status.ToString()
                    ))
                    .SingleOrDefaultAsync(ct);

                return device ?? throw new DeviceNotFoundException(query.Id);
            },
            tags: [CacheTags.Devices],
            token: cancellationToken
        );
    }
}