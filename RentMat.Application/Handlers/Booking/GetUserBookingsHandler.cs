using Microsoft.EntityFrameworkCore;
using RentMat.Application.DTOs.RentalBooking;
using RentMat.Application.Exceptions;
using RentMat.Infrastructure.Data;
using ZiggyCreatures.Caching.Fusion;

namespace RentMat.Application.Handlers.Booking;

public class GetUserBookingsHandler
{
    private readonly AppDbContext _db;

    public GetUserBookingsHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<BookingResponseDto>> Handle(int userId, CancellationToken cancellationToken)
    {
        var login = await _db.Users.Where(u => u.Id == userId)
            .Select(u => u.Login)
            .SingleOrDefaultAsync(cancellationToken);

        if (login == null)
            throw new UserNotFoundException(userId);

        return await _db.Bookings
            .AsNoTracking()
            .Where(b => b.UserId == userId)
            .Select(b => new BookingResponseDto(b.Id, b.Device.Name, login,
                b.Status.ToString(), b.StartDate, b.EndDate, b.TotalPrice))
            .ToListAsync(cancellationToken);
    }
}