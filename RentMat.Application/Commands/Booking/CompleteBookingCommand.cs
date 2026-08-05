using MediatR;

namespace RentMat.Application.Commands.Booking;

public record CompleteBookingCommand(int BookingId, int UserId) : IRequest;