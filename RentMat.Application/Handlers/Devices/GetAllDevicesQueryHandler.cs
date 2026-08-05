using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentMat.Application.Common;
using RentMat.Application.DTOs.Device;
using RentMat.Application.Queries.Devices;
using RentMat.Application.Queries.Users;
using RentMat.Core.Enums;
using RentMat.Infrastructure.Data;
using ZiggyCreatures.Caching.Fusion;

namespace RentMat.Application.Handlers.Devices;

public class GetAllDevicesQueryHandler : IRequestHandler<GetAllDevicesQuery, PagedResponse<DeviceResponseDto>>
{
    public const int DefaultPageSize = 10;
    public const int MaxPageSize = 50;
    
    private readonly IFusionCache _cache;

    private readonly AppDbContext _db;
    private readonly ILogger<GetAllDevicesQueryHandler> _logger;

    public GetAllDevicesQueryHandler(AppDbContext db, IFusionCache cache, ILogger<GetAllDevicesQueryHandler> logger)
    {
        _cache = cache;
        _db = db;
        _logger = logger;
    }

    public async Task<PagedResponse<DeviceResponseDto>> Handle(
        GetAllDevicesQuery query,
        CancellationToken cancellationToken)
    {
        var search = query.Search?.Trim().ToLowerInvariant();
        var status = query.Status;

        var cacheKey =
            $"devices:page:{query.Page}:page-size:{query.PageSize}:search:{search ?? string.Empty}:status:{status?.ToString() ?? "all"}";

        return await _cache.GetOrSetAsync<PagedResponse<DeviceResponseDto>>(
            cacheKey,
            async (ctx, ct) =>
            {
                _logger.LogDebug("Cache miss for key {CacheKey}", cacheKey);

                var devicesQuery = _db.Devices
                    .AsNoTracking();

                if (!string.IsNullOrWhiteSpace(search))
                    devicesQuery = devicesQuery.Where(d => EF.Functions.ILike(d.Name, $"%{search}%"));

                devicesQuery = status != null
                    ? devicesQuery.Where(d => d.Status == status)
                    : devicesQuery.Where(d => d.Status != DeviceStatus.Retired);

                var totalItems = await devicesQuery.CountAsync(ct);
                var totalPages = (int)Math.Ceiling((double)totalItems / query.PageSize);

                var devices = await devicesQuery
                    .OrderBy(d => d.Id)
                    .Skip((query.Page - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .Select(d => new DeviceResponseDto(
                        d.Id,
                        d.Name,
                        d.HourRentPrice,
                        d.Category.Name,
                        d.Status.ToString()
                    ))
                    .ToListAsync(ct);

                return new PagedResponse<DeviceResponseDto>
                {
                    Items = devices,
                    Page = query.Page,
                    PageSize = query.PageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages
                };
            },
            tags: [CacheTags.Devices],
            token: cancellationToken);
    }
}