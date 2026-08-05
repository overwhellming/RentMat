using FluentValidation;
using RentMat.Application.Commands.Booking;

namespace RentMat.Application.Validators.Booking;

public class CompleteBookingCommandValidator : AbstractValidator<CompleteBookingCommand>
{
    public CompleteBookingCommandValidator()
    {
        RuleFor(x => x.BookingId)
            .GreaterThan(0);
        RuleFor(x => x.UserId)
            .GreaterThan(0);
    }
}