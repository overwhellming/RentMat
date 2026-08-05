using MediatR;
using Microsoft.EntityFrameworkCore;
using RentMat.Application.Commands.Booking;
using RentMat.Application.Common;
using RentMat.Application.Exceptions.Booking;
using RentMat.Core.Enums;
using RentMat.Infrastructure.Data;
using ZiggyCreatures.Caching.Fusion;

namespace RentMat.Application.Handlers.Booking;

public class CompleteBookingCommandHandler : IRequestHandler<CompleteBookingCommand>
{
    private readonly IFusionCache _cache;
    private readonly AppDbContext _db;

    public CompleteBookingCommandHandler(AppDbContext db, IFusionCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task Handle(CompleteBookingCommand command, CancellationToken cancellationToken)
    {
        var activeBooking =
            await _db.Bookings
                .Include(b => b.Device)
                .FirstOrDefaultAsync(b => b.Id == command.BookingId && b.Status == BookingStatus.Active
                    , cancellationToken);

        if (activeBooking == null)
            throw new ActiveBookingNotFoundException(command.BookingId);

        if (activeBooking.UserId != command.UserId)
            throw new BookingAccessDeniedException();

        activeBooking.EndDate = DateTimeOffset.UtcNow;
        activeBooking.Status = BookingStatus.Completed;
        activeBooking.Device.Status = DeviceStatus.Available;

        await _db.SaveChangesAsync(cancellationToken);
        await _cache.RemoveByTagAsync(CacheTags.Bookings);
    }
}