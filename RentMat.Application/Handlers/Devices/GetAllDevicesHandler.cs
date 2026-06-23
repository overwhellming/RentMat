using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentMat.Application.Common;
using RentMat.Application.DTOs.Device;
using RentMat.Core.Enums;
using RentMat.Infrastructure.Data;
using ZiggyCreatures.Caching.Fusion;

namespace RentMat.Application.Handlers.Devices;

public class GetAllDevicesHandler
{
    private const int DefaultPageSize = 10;
    private const int MaxPageSize = 50;
    
    private readonly AppDbContext _db;
    private readonly IFusionCache _cache;
    private readonly ILogger<GetAllDevicesHandler> _logger;

    public GetAllDevicesHandler(IFusionCache cache, AppDbContext db, ILogger<GetAllDevicesHandler> logger)
    {
        _cache = cache;
        _db = db;
        _logger = logger;
    }

    public async Task<PagedResponse<DeviceResponseDto>> Handle(
        int page,
        int pageSize,
        string? search,
        DeviceStatus? status,
        CancellationToken cancellationToken)
    {
        if (page < 1)
            page = 1;
        if (pageSize < 1)
            pageSize = DefaultPageSize;
        else if (pageSize > MaxPageSize)
            pageSize = MaxPageSize;

        search = search?.Trim().ToLowerInvariant();
        
        var cacheKey =
            $"devices:page:{page}:page-size:{pageSize}:search:{search ?? string.Empty}:status:{status?.ToString() ?? "all"}";

        return await _cache.GetOrSetAsync<PagedResponse<DeviceResponseDto>>(
            cacheKey,
            async (ctx, ct) =>
            {
                _logger.LogDebug("Cache miss for key {CacheKey}", cacheKey);
                
                var query = _db.Devices
                    .AsNoTracking();

                if (!string.IsNullOrWhiteSpace(search))
                    query = query.Where(d => EF.Functions.ILike(d.Name, $"%{search}%"));
                
                if (status != null)
                    query = query.Where(d => d.Status == status);
                else
                    query = query.Where(d => d.Status != DeviceStatus.Retired);

                var totalItems = await query.CountAsync(ct);
                var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                var devices = await query
                    .OrderBy(d => d.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
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
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages
                };
            },
            tags: [CacheTags.Devices],
            token: cancellationToken);
    }
}