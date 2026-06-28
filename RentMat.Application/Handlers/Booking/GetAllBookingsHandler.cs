using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentMat.Application.Common;
using RentMat.Application.DTOs.RentalBooking;
using RentMat.Application.Queries;
using RentMat.Core.Enums;
using RentMat.Infrastructure.Data;
using ZiggyCreatures.Caching.Fusion;

namespace RentMat.Application.Handlers.Booking;

public class GetAllBookingsHandler
{
    private const int DefaultPageSize = 10;
    private const int MaxPageSize = 50;
    private readonly IFusionCache _cache;

    private readonly AppDbContext _db;
    private readonly ILogger<GetAllBookingsHandler> _logger;

    public GetAllBookingsHandler(AppDbContext db, IFusionCache cache, ILogger<GetAllBookingsHandler> logger)
    {
        _db = db;
        _cache = cache;
        _logger = logger;
    }

    public async Task<PagedResponse<BookingResponseDto>> Handle(
        GetAllBookingsQuery query,
        CancellationToken cancellationToken)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1
            ? DefaultPageSize
            : Math.Min(query.PageSize, MaxPageSize);
        var search = query.Search?.Trim().ToLowerInvariant();
        var status = query.Status;

        var cacheKey =
            $"bookings:page:{page}:page-size:{pageSize}:search:{search ?? string.Empty}:status:{status?.ToString() ?? "all"}";

        return await _cache.GetOrSetAsync<PagedResponse<BookingResponseDto>>(
            cacheKey,
            async (ctx, ct) =>
            {
                _logger.LogDebug("Cache miss for key {CacheKey}", cacheKey);

                var bookingsQuery = _db.Bookings
                    .AsNoTracking();

                if (!string.IsNullOrWhiteSpace(search))
                    bookingsQuery = bookingsQuery.Where(b => EF.Functions.ILike(b.Device.Name, $"%{search}%"));
                if (status != null)
                    bookingsQuery = bookingsQuery.Where(b => b.Status == status);

                var totalItems = await bookingsQuery.CountAsync(ct);
                var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                var items = await bookingsQuery
                    .OrderBy(b => b.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(b =>
                        new BookingResponseDto(b.Id, b.Device.Name, b.User.Login, b.Status.ToString(), b.StartDate,
                            b.EndDate, b.TotalPrice))
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