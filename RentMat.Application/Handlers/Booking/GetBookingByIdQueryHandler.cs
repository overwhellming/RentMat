using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentMat.Application.Common;
using RentMat.Application.DTOs.RentalBooking;
using RentMat.Application.Exceptions.Booking;
using RentMat.Application.Queries.Booking;
using RentMat.Infrastructure.Data;
using ZiggyCreatures.Caching.Fusion;

namespace RentMat.Application.Handlers.Booking;

public class GetBookingByIdQueryHandler : IRequestHandler<GetBookingByIdQuery, BookingResponseDto>
{
    private readonly IFusionCache _cache;
    private readonly AppDbContext _db;
    private readonly ILogger<GetBookingByIdQueryHandler> _logger;

    public GetBookingByIdQueryHandler(AppDbContext db, IFusionCache cache, ILogger<GetBookingByIdQueryHandler> logger)
    {
        _db = db;
        _cache = cache;
        _logger = logger;
    }

    public async Task<BookingResponseDto> Handle(GetBookingByIdQuery query, CancellationToken cancellationToken)
    {
        var cacheKey = $"bookings:id:{query.Id}";

        return await _cache.GetOrSetAsync<BookingResponseDto>(
            cacheKey,
            async (ctx, ct) =>
            {
                _logger.LogDebug("Cache miss for key {CacheKey}", cacheKey);

                var booking = await _db.Bookings
                    .AsNoTracking()
                    .Where(b => b.Id == query.Id)
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

                return booking ?? throw new BookingNotFoundException(query.Id);
            },
            tags: [CacheTags.Bookings],
            token: cancellationToken
        );
    }
}