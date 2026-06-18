using System.Data;
using Microsoft.EntityFrameworkCore;
using RentMat.Application.Exceptions;
using RentMat.Application.DTOs.RentalBooking;
using RentMat.Core.Enums;
using RentMat.Core.Models;
using RentMat.Infrastructure.Data;
using ZiggyCreatures.Caching.Fusion;

namespace RentMat.Application.Handlers.Booking;

public class BookDeviceHandler
{
    private const int MinutesBetweenRents = 15;
    private readonly AppDbContext _db;
    private readonly IFusionCache _cache;
    
    public BookDeviceHandler(AppDbContext db, IFusionCache cache)
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

        var hasConflicts = await _db.Bookings.AnyAsync(r =>
            r.DeviceId == dto.DeviceId &&
            (r.Status == BookingStatus.Active || r.Status == BookingStatus.Created) &&
            dto.StartDate < r.EndDate.AddMinutes(MinutesBetweenRents) &&
            dto.EndDate > r.StartDate.AddMinutes(-MinutesBetweenRents), cancellationToken);

        if (!hasConflicts)
        {
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
            await _db.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
            
            await _cache.RemoveByTagAsync("bookings");
            
            return new BookingResponseDto(booking.Id, device.Name, user.Login, booking.Status.ToString(), dto.StartDate, dto.EndDate, totalPrice);
        }
        else
        {
            throw new DeviceAlreadyBookedException(device.Id);
        }
    }
}