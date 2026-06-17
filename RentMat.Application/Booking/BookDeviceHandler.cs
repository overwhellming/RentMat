using System.Data;
using Microsoft.EntityFrameworkCore;
using RentMat.Application.Booking.Exceptions;
using RentMat.Application.DTOs.RentalBooking;
using RentMat.Core.Enums;
using RentMat.Core.Models;
using RentMat.Infrastructure.Data;
using ZiggyCreatures.Caching.Fusion;

namespace RentMat.Application.Booking;

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

    public async Task<BookingResponseDto> Handle(BookingCreateDto dto,
        CancellationToken cancellationToken = default)
    {
        if (dto.endDate <= dto.startDate)
            throw new InvalidDateRangeException();

        await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == dto.userId, cancellationToken) ??
                   throw new UserNotFoundException(dto.userId);

        var device = await _db.Devices
                         .AsNoTracking()
                         .FirstOrDefaultAsync(d => d.Id == dto.deviceId, cancellationToken) ??
                     throw new DeviceNotFoundException(dto.deviceId);

        var hasConflicts = await _db.Bookings.AnyAsync(r =>
            r.DeviceId == dto.deviceId &&
            (r.Status == BookingStatus.Active || r.Status == BookingStatus.Created) &&
            dto.startDate < r.EndDate.AddMinutes(MinutesBetweenRents) &&
            dto.endDate > r.StartDate.AddMinutes(-MinutesBetweenRents), cancellationToken);

        if (!hasConflicts)
        {
            var priceForHour = device.HourRentPrice;
            var hours = (decimal)(dto.endDate - dto.startDate).TotalMinutes / 60m;
            var totalPrice = hours * priceForHour;

            if (user.Balance < totalPrice)
                throw new NotEnoughMoneyException(user.Id);

            user.Balance -= totalPrice;
            var booking = new Core.Models.Booking
            {
                DeviceId = dto.deviceId,
                UserId = dto.userId,
                StartDate = dto.startDate,
                EndDate = dto.endDate,
                TotalPrice = totalPrice,
                Status = BookingStatus.Created
            };
            
            _db.Bookings.Add(booking);
            await _db.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);

            await _cache.RemoveByTagAsync("bookings");
            
            return new BookingResponseDto(booking.Id, device.Name, user.Login, booking.Status.ToString(), dto.startDate, dto.endDate, totalPrice);
        }
        else
        {
            throw new DeviceAlreadyBookedException(device.Id);
        }
    }
}