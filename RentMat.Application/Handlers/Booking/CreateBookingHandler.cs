using System.Data;
using Microsoft.EntityFrameworkCore;
using RentMat.Application.Common;
using RentMat.Application.DTOs.RentalBooking;
using RentMat.Application.Exceptions.Devices;
using RentMat.Application.Exceptions.Users;
using RentMat.Core.Enums;
using RentMat.Infrastructure.Data;
using ZiggyCreatures.Caching.Fusion;

namespace RentMat.Application.Handlers.Booking;

public class CreateBookingHandler
{
    private const int MinutesBetweenRents = 15;
    private readonly IFusionCache _cache;
    private readonly AppDbContext _db;

    public CreateBookingHandler(AppDbContext db, IFusionCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<BookingResponseDto> Handle(BookingCreateDto dto, int userId,
        CancellationToken cancellationToken = default)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken) ??
                   throw new UserNotFoundException(userId);

        var device = await _db.Devices
                         .AsNoTracking()
                         .FirstOrDefaultAsync(d => d.Id == dto.DeviceId, cancellationToken) ??
                     throw new DeviceNotFoundException(dto.DeviceId);

        if (device.Status != DeviceStatus.Available)
            throw new DeviceIsNotAvailableException(dto.DeviceId);

        var buffer = TimeSpan.FromMinutes(MinutesBetweenRents);
        var hasConflicts = await _db.Bookings.AnyAsync(r =>
                r.DeviceId == dto.DeviceId &&
                (r.Status == BookingStatus.Active || r.Status == BookingStatus.Created) &&
                dto.StartDate < r.EndDate + buffer &&
                dto.EndDate > r.StartDate - buffer,
            cancellationToken);

        if (hasConflicts) throw new DeviceIsBookedException(device.Id);

        var priceForHour = device.HourRentPrice;
        var hours = (decimal)(dto.EndDate - dto.StartDate).TotalMinutes / 60m;
        var totalPrice = hours * priceForHour;

        if (user.Balance < totalPrice)
            throw new NotEnoughMoneyException(user.Id);

        user.Balance -= totalPrice;
        var booking = new Core.Models.Booking
        {
            DeviceId = dto.DeviceId,
            UserId = userId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            TotalPrice = totalPrice,
            Status = BookingStatus.Created
        };

        _db.Bookings.Add(booking);
        device.Status = DeviceStatus.Rented;

        await _db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);

        await _cache.RemoveByTagAsync(CacheTags.Bookings);

        return new BookingResponseDto(booking.Id, device.Name, user.Login, booking.Status.ToString(), dto.StartDate,
            dto.EndDate, totalPrice);
    }
}