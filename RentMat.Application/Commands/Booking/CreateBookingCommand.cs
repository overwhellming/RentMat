using MediatR;
using RentMat.Application.DTOs.RentalBooking;

namespace RentMat.Application.Commands.Booking;

public record CreateBookingCommand(int DeviceId, int UserId, DateTimeOffset StartDate, DateTimeOffset EndDate)
    : IRequest<BookingResponseDto>;