using FluentValidation;
using RentMat.Application.Handlers.Devices;
using RentMat.Application.Queries.Booking;
using RentMat.Core.Constants;

namespace RentMat.Application.Validators.Devices;

public class GetAllBookingsQueryValidator : AbstractValidator<GetAllBookingsQuery>
{
    public GetAllBookingsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, GetAllDevicesQueryHandler.MaxPageSize);

        RuleFor(x => x.Search)
            .MaximumLength(ValidationConstants.DeviceSearchMaxLength)
            .When(x => x.Search is not null);
    }
}