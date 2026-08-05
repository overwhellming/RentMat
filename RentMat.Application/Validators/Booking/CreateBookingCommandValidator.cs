using FluentValidation;
using RentMat.Application.Commands.Booking;

namespace RentMat.Application.Validators.Booking;

public class CreateBookingCommandValidator : AbstractValidator<CreateBookingCommand>
{
    public CreateBookingCommandValidator()
    {
        RuleFor(x => x.DeviceId)
            .GreaterThan(0);

        RuleFor(x => x.UserId)
            .GreaterThan(0);

        RuleFor(x => x.StartDate)
            .Cascade(CascadeMode.Stop)
            .GreaterThanOrEqualTo(_ => DateTimeOffset.UtcNow)
            .WithMessage("Start date cannot be in the past")
            .LessThan(x => x.EndDate)
            .WithMessage("Start date must be before the end date");
    }
}