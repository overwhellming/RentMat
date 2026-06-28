using FluentValidation;
using RentMat.Application.DTOs.RentalBooking;

namespace RentMat.Application.Validators.Booking;

public class BookingCreateDtoValidator : AbstractValidator<BookingCreateDto>
{
    public BookingCreateDtoValidator()
    {
        RuleFor(x => x.StartDate)
            .GreaterThanOrEqualTo(x => DateTimeOffset.UtcNow)
            .LessThan(x => x.EndDate)
            .WithMessage("Start date must be in the future and before the end date");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .WithMessage("End date must be after the start date");
    }
}