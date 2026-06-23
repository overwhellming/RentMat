using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentMat.Application.Common;
using RentMat.Application.DTOs.RentalBooking;
using RentMat.Application.Exceptions.Booking;
using RentMat.Infrastructure.Data;
using ZiggyCreatures.Caching.Fusion;

namespace RentMat.Application.Handlers.Booking;

public class GetBookingByIdHandler
{
    private readonly AppDbContext _db;
    private readonly IFusionCache _cache;
    private readonly ILogger<GetBookingByIdHandler> _logger;

    public GetBookingByIdHandler(AppDbContext db, IFusionCache cache, ILogger<GetBookingByIdHandler> logger)
    {
        _db = db;
        _cache = cache;
        _logger = logger;
    }

    public async Task<BookingResponseDto> Handle(int bookingId, CancellationToken cancellationToken)
    {
        var cacheKey = $"bookings:id:{bookingId}";

        return await _cache.GetOrSetAsync<BookingResponseDto>(
            cacheKey,
            async (ctx, ct) =>
            {
                _logger.LogDebug("Cache miss for key {CacheKey}", cacheKey);
                
                var booking = await _db.Bookings
                    .AsNoTracking()
                    .Where(b => b.Id == bookingId)
                    .Select(b => new BookingResponseDto(
                        b.Id,
                        b.Device.Name,
                        b.User.Login,
                        b.Status.ToString(),
                        b.StartDate,
                        b.EndDate,
                        b.TotalPrice
                    ))
                    .SingleOrDefaultAsync(ct);

                return booking ?? throw new BookingNotFoundException(bookingId);
            },
            tags: [CacheTags.Bookings],
            token: cancellationToken
        );
    }
}