using FluentValidation;
using RentMat.Application.Handlers.Devices;
using RentMat.Application.Queries.Users;
using RentMat.Core.Constants;

namespace RentMat.Application.Validators.Devices;

public class GetAllUsersQueryValidator : AbstractValidator<GetAllUsersQuery>
{
    public GetAllUsersQueryValidator()
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