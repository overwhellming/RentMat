using FluentValidation;
using RentMat.Application.Commands.Devices;

namespace RentMat.Application.Validators.Devices;

public class RetireDeviceCommandValidator : AbstractValidator<RetireDeviceCommand>
{
    public RetireDeviceCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);
    }
}