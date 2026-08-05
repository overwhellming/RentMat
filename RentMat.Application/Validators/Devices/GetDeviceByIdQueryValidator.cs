using FluentValidation;
using RentMat.Application.Queries.Devices;

namespace RentMat.Application.Validators.Devices;

public class GetDeviceByIdQueryValidator : AbstractValidator<GetDeviceByIdQuery>
{
    public GetDeviceByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);
    }
}