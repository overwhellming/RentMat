using FluentValidation;
using RentMat.Application.Queries.Booking;

namespace RentMat.Application.Validators.Booking;

public class GetUserBookingsQueryValidator : AbstractValidator<GetUserBookingsQuery>
{
    public GetUserBookingsQueryValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0);
    }
}