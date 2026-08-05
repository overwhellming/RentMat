using MediatR;
using Microsoft.EntityFrameworkCore;
using RentMat.Application.DTOs.RentalBooking;
using RentMat.Application.Exceptions.Users;
using RentMat.Application.Queries.Booking;
using RentMat.Infrastructure.Data;

namespace RentMat.Application.Handlers.Booking;

public class GetUserBookingsQueryHandler : IRequestHandler<GetUserBookingsQuery, IEnumerable<BookingResponseDto>>
{
    private readonly AppDbContext _db;

    public GetUserBookingsQueryHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<BookingResponseDto>> Handle(GetUserBookingsQuery query, CancellationToken cancellationToken)
    {
        var login = await _db.Users.Where(u => u.Id == query.UserId)
            .Select(u => u.Login)
            .SingleOrDefaultAsync(cancellationToken);

        if (login == null)
            throw new UserNotFoundException(query.UserId);

        return await _db.Bookings
            .AsNoTracking()
            .Where(b => b.UserId == query.UserId)
            .Select(b => new BookingResponseDto(b.Id, b.Device.Name, login,
                b.Status.ToString(), b.StartDate, b.EndDate, b.TotalPrice))
            .ToListAsync(cancellationToken);
    }
}