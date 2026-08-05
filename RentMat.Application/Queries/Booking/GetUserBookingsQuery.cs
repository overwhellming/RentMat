using MediatR;
using RentMat.Application.DTOs.RentalBooking;

namespace RentMat.Application.Queries.Booking;

public record GetUserBookingsQuery(int UserId) : IRequest<IEnumerable<BookingResponseDto>>;