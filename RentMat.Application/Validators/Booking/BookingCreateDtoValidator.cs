using FluentValidation;
using RentMat.Application.DTOs.RentalBooking;

namespace RentMat.Application.Validators.Booking;

public class BookingCreateDtoValidator : AbstractValidator<BookingCreateDto>
{
    public BookingCreateDtoValidator()
    {
        RuleFor(x => x.DeviceId)
            .GreaterThan(0);
        
        RuleFor(x => x.StartDate)
            .Cascade(CascadeMode.Stop)
            .GreaterThanOrEqualTo(_ => DateTimeOffset.UtcNow)
            .WithMessage("Start date cannot be in the past")
            .LessThan(x => x.EndDate)
            .WithMessage("Start date must be before the end date");
    }
}