using FluentValidation;
using RentMat.Application.Commands.Devices;
using RentMat.Application.Queries.Devices;

namespace RentMat.Application.Validators.Devices;

public class RetireDeviceCommandValidator : AbstractValidator<RetireDeviceCommand>
{
    public RetireDeviceCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);
    }
}