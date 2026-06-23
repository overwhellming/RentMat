using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentMat.Application.Common;
using RentMat.Application.DTOs.RentalBooking;
using RentMat.Core.Enums;
using RentMat.Infrastructure.Data;
using ZiggyCreatures.Caching.Fusion;

namespace RentMat.Application.Handlers.Booking;

public class GetAllBookingsHandler
{
    private const int DefaultPageSize = 10;
    private const int MaxPageSize = 50;

    private readonly AppDbContext _db;
    private readonly IFusionCache _cache;
    private readonly ILogger<GetAllBookingsHandler> _logger;
    
    public GetAllBookingsHandler(AppDbContext db, IFusionCache cache, ILogger<GetAllBookingsHandler> logger)
    {
        _db = db;
        _cache = cache;
        _logger = logger;
    }

    public async Task<PagedResponse<BookingResponseDto>> Handle(
        int page,
        int pageSize,
        string? search,
        BookingStatus? status,
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
            $"bookings:page:{page}:page-size:{pageSize}:search:{search ?? string.Empty}:status:{status?.ToString() ?? "all"}";

        return await _cache.GetOrSetAsync<PagedResponse<BookingResponseDto>>(
            cacheKey,
            async (ctx, ct) =>
            {
                _logger.LogDebug("Cache miss for key {CacheKey}", cacheKey);
                
                IQueryable<Core.Models.Booking> query = _db.Bookings
                    .AsNoTracking()
                    .Include(b => b.Device);
                
                if (!string.IsNullOrWhiteSpace(search))
                    query = query.Where(b => EF.Functions.ILike(b.Device.Name, $"%{search}%"));
                if (status != null)
                    query = query.Where(b => b.Status == status);

                var totalItems = await query.CountAsync(ct);
                var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                var items = await query
                    .OrderBy(b => b.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(b =>
                        new BookingResponseDto(b.Id, b.Device.Name, b.User.Login, b.Status.ToString(), b.StartDate, b.EndDate, b.TotalPrice))
                    .ToListAsync(ct);

                return new PagedResponse<BookingResponseDto>
                {
                    Items = items,
                    TotalItems = totalItems,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = totalPages
                };
            },
            tags: [CacheTags.Bookings],
            token: cancellationToken);
    }
}