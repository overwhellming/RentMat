using FluentValidation;
using RentMat.Application.Commands.Devices;
using RentMat.Application.DTOs.Device;
using RentMat.Core.Constants;

namespace RentMat.Application.Validators.Devices;

public class UpdateDeviceCommandValidator : AbstractValidator<UpdateDeviceCommand>
{
    public UpdateDeviceCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);
        
        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MaximumLength(ValidationConstants.DeviceNameMaxLength);
        
        RuleFor(x => x.HourRentPrice)
            .GreaterThan(0);
        
        RuleFor(x => x.CategoryId)
            .GreaterThan(0);
    }
}