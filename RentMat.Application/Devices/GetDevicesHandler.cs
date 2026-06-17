using Microsoft.EntityFrameworkCore;
using RentMat.Application.Common;
using RentMat.Application.DTOs.Device;
using RentMat.Core.Enums;
using RentMat.Infrastructure.Data;
using ZiggyCreatures.Caching.Fusion;

namespace RentMat.Application.Devices;

public class GetDevicesHandler
{
    private const int DefaultPageSize = 10;
    private const int MaxPageSize = 50;
    
    private readonly AppDbContext _db;
    private readonly IFusionCache _cache;

    public GetDevicesHandler(IFusionCache cache, AppDbContext db)
    {
        _cache = cache;
        _db = db;
    }

    public async Task<PagedResponse<DeviceResponseDto>> Handle(
        int page,
        int pageSize,
        string? search,
        DeviceStatus? status,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
            page = 1;
        if (pageSize < 1)
            pageSize = DefaultPageSize;
        else if (pageSize > MaxPageSize)
            pageSize = 50;

        var cacheKey =
            $"devices:page:{page}:page-size:{pageSize}:search:{search ?? string.Empty}:status:{status?.ToString() ?? "all"}";

        return await _cache.GetOrSetAsync<PagedResponse<DeviceResponseDto>>(
            cacheKey,
            async (ctx, ct) =>
            {
                var query = _db.Devices
                    .AsNoTracking();

                if (!string.IsNullOrWhiteSpace(search))
                    query = query.Where(d => EF.Functions.ILike(d.Name, $"%{search}%"));
                if (status != null)
                    query = query.Where(d => d.Status == status);

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
            tags: ["devices"],
            token: cancellationToken);
    }
}