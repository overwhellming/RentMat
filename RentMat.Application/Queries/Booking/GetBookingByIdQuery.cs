using MediatR;
using RentMat.Application.DTOs.RentalBooking;

namespace RentMat.Application.Queries.Booking;

public record GetBookingByIdQuery(int Id) : IRequest<BookingResponseDto>;